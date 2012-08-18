using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public class MethodProcessor
{
   public  ReferenceFinder referenceFinder;
   public TypeSystem typeSystem;

    public InterceptorFinder InterceptorFinder;


    public void Process(IEnumerable<MethodDefinition> methods)
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
            InnerProcessNonVoid(method);
        }
        catch (Exception exception)
        {
            throw new WeavingException(string.Format("An error occurred processing '{0}'. Error: {1}", method.FullName, exception.Message));
        }
    }

    void InnerProcessNonVoid(MethodDefinition method)
    {
        var body = method.Body;
        body.SimplifyMacros();
        var ilProcessor = body.GetILProcessor();

        var instructions = body.Instructions;

        var handlerEnd = FixReturns(method);
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

    Instruction FixReturns(MethodDefinition method)
    {
        if (method.ReturnType == typeSystem.Void)
        {
            var instructions = method.Body.Instructions;
            var lastRet = Instruction.Create(OpCodes.Ret);
            instructions.Add(lastRet);

            for (var index = 0; index < instructions.Count - 1; index++)
            {
                var instruction = instructions[index];
                if (instruction.OpCode == OpCodes.Ret)
                {
                    instructions[index] = Instruction.Create(OpCodes.Leave, lastRet);
                }
            }
            return lastRet;
        }
        else
        {
            var instructions = method.Body.Instructions;
            var returnVariable = new VariableDefinition("methodTimerReturn", method.ReturnType);
            method.Body.Variables.Add(returnVariable);
            var lastLd = Instruction.Create(OpCodes.Ldloc, returnVariable);
            instructions.Add(lastLd);
            instructions.Add(Instruction.Create(OpCodes.Ret));

            for (var index = 0; index < instructions.Count - 2; index++)
            {
                var instruction = instructions[index];
                if (instruction.OpCode == OpCodes.Ret)
                {
                    instructions[index] = Instruction.Create(OpCodes.Leave, lastLd);
                    instructions.Insert(index, Instruction.Create(OpCodes.Stloc, returnVariable));
                    index++;
                }
            }

            return lastLd;
        }
    }

    static void InjectWriteIl(List<Instruction> writeTimeIl, ILProcessor ilProcessor, Instruction beforeThis)
    {
        foreach (var instruction in writeTimeIl)
        {
            ilProcessor.InsertBefore(beforeThis, instruction);
        }
        ilProcessor.InsertBefore(beforeThis, Instruction.Create(OpCodes.Endfinally));
    }

    List<Instruction> GetWriteTimeIL(MethodDefinition method, VariableDefinition stopwatchVar)
    {
        if (InterceptorFinder.LogMethod == null)
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
                    Instruction.Create(OpCodes.Call, referenceFinder.DebugWriteLineMethod),
                };
        }
        return new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldloc, stopwatchVar),
                Instruction.Create(OpCodes.Call, referenceFinder.StopMethod),

                //ldtoken method int32 Program::Foo()  
                //call class [mscorlib]System.Reflection.MethodBase [mscorlib]System.Reflection.MethodBase::GetMethodFromHandle(valuetype [mscorlib]System.RuntimeMethodHandle)  
                //castclass  [mscorlib]System.Reflection.MethodInfo  
                Instruction.Create(OpCodes.Ldtoken, method),
                Instruction.Create(OpCodes.Call, referenceFinder.GetMethodFromHandle),
                Instruction.Create(OpCodes.Castclass, referenceFinder.MethodInfoType),

                Instruction.Create(OpCodes.Ldloc, stopwatchVar),
                Instruction.Create(OpCodes.Call, referenceFinder.ElapsedMilliseconds),   
                Instruction.Create(OpCodes.Call, InterceptorFinder.LogMethod),   
            };
    }

    VariableDefinition InjectStopwatch(MethodBody body)
    {
        var stopwatchVar = new VariableDefinition("methodTimerStopwatch", referenceFinder.StopwatchType);
        body.Variables.Add(stopwatchVar);


        body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, referenceFinder.StartNewMethod));
        body.Instructions.Insert(1, Instruction.Create(OpCodes.Stloc, stopwatchVar));
        return stopwatchVar;
    }
}