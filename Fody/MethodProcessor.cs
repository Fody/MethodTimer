using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;


public class MethodProcessor
{
    ReferenceFinder referenceFinder;
    TypeSystem typeSystem;

    public MethodProcessor(ModuleDefinition moduleDefinition, ReferenceFinder referenceFinder)
    {
        this.referenceFinder = referenceFinder;
        typeSystem = moduleDefinition.TypeSystem;
    }

    public void Process(IEnumerable< MethodDefinition> methods)
    {
        foreach (var method in methods)
        {
            Process(method);
        }
    }

    public void Process(MethodDefinition method)
    {
        try
        {
            if (method.ReturnType == typeSystem.Void)
            {
                InnerProcessVoid(method);
            }
            else
            {

                InnerProcessNonVoid(method);
            }
        }
        catch (Exception exception)
        {
            throw new WeavingException(string.Format("An error occurred processing '{0}'. Error: {1}", method.FullName, exception.Message));
        }
    }

    void InnerProcessVoid(MethodDefinition method)
    {
        var body = method.Body;
        body.SimplifyMacros();
        var ilProcessor = body.GetILProcessor();
        

        var instructions = body.Instructions;

        var lastRet = Instruction.Create(OpCodes.Ret);
        instructions.Add(lastRet);

        ForwardRetToLeave(instructions, lastRet);

        var tryStart = instructions.First();

        var stopwatchVar = InjectStopwatch(body); 
        
        
        var writeTimeIl = GetWriteTimeIL(method, stopwatchVar);
        
        InjectWriteIl(writeTimeIl, ilProcessor, lastRet);

        var handler = new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = tryStart,
            TryEnd = writeTimeIl.First(),
            HandlerStart = writeTimeIl.First(),
            HandlerEnd = lastRet,
        };

        body.ExceptionHandlers.Add(handler);
        body.InitLocals = true;
        body.OptimizeMacros();
    }
    void InnerProcessNonVoid(MethodDefinition method)
    {
        var body = method.Body;
        body.SimplifyMacros();
        var ilProcessor = body.GetILProcessor();

        var returnVariable = new VariableDefinition(method.ReturnType);
        body.Variables.Add(returnVariable);

        var instructions = body.Instructions;

        var lastLd = Instruction.Create(OpCodes.Ldloc, returnVariable);
        instructions.Add(lastLd);
        instructions.Add(Instruction.Create(OpCodes.Ret));

        for (var index = 0; index < instructions.Count - 2; index++)
        {
            var instruction = instructions[index];
            if (instruction.OpCode == OpCodes.Ret)
            {
                instructions[index] = Instruction.Create(OpCodes.Leave, lastLd);
                instructions.Insert(index,Instruction.Create(OpCodes.Stloc, returnVariable));
                index++;
            }
        }

        var handlerEnd = lastLd;
        var tryStart = instructions.First();

        var stopwatchVar = InjectStopwatch(body); 
        
        var writeTimeIl = GetWriteTimeIL(method, stopwatchVar);

        InjectWriteIl(writeTimeIl, ilProcessor, handlerEnd);

        var handler = new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = tryStart,
            TryEnd = writeTimeIl.First(),
            HandlerStart = writeTimeIl.First(),
            HandlerEnd = handlerEnd,
        };

        body.ExceptionHandlers.Add(handler);
        body.InitLocals = true;
        body.OptimizeMacros();
    }

    static void InjectWriteIl(List<Instruction> writeTimeIl, ILProcessor ilProcessor, Instruction beforeThis)
    {
        foreach (var instruction in writeTimeIl)
        {
            ilProcessor.InsertBefore(beforeThis, instruction);
        }
        ilProcessor.InsertBefore(beforeThis, Instruction.Create(OpCodes.Endfinally));
    }

    static void ForwardRetToLeave(Collection<Instruction> instructions, Instruction forwardToThis)
    {
        for (var index = 0; index < instructions.Count - 1; index++)
        {
            var instruction = instructions[index];
            if (instruction.OpCode == OpCodes.Ret)
            {
                instructions[index] = Instruction.Create(OpCodes.Leave, forwardToThis);
            }
        }
    }


    List<Instruction> GetWriteTimeIL(MethodDefinition method, VariableDefinition stopwatchVar)
    {
        return new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldloc, stopwatchVar),
                Instruction.Create(OpCodes.Call, referenceFinder.StopMethod),
                Instruction.Create(OpCodes.Ldstr, string.Format("{0}.{1} ", method.DeclaringType.Name, method.Name)),
                Instruction.Create(OpCodes.Ldloc, stopwatchVar),
                Instruction.Create(OpCodes.Call, referenceFinder.ElapsedMilliseconds),
                Instruction.Create(OpCodes.Box, typeSystem.Int64),
                Instruction.Create(OpCodes.Ldstr, "ms"),
                Instruction.Create(OpCodes.Call, referenceFinder.ConcatMethod),
                Instruction.Create(OpCodes.Call, referenceFinder.WriteLineMethod),
            };
    }

    VariableDefinition InjectStopwatch(MethodBody body)
    {
        var stopwatchVar = new VariableDefinition(referenceFinder.StopwatchType);
        body.Variables.Add(stopwatchVar);


        body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, referenceFinder.StartNewMethod));
        body.Instructions.Insert(1, Instruction.Create(OpCodes.Stloc, stopwatchVar));
        return stopwatchVar;
    }
}