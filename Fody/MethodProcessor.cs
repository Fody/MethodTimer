using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public class MethodProcessor
{
   public  ReferenceFinder ReferenceFinder;
   public TypeSystem TypeSystem;
    public InterceptorFinder InterceptorFinder;
    public MethodDefinition Method;
    MethodBody body;
    ILProcessor ilProcessor;

    public void Process()
    {
        try
        {
            InnerProcess();
        }
        catch (Exception exception)
        {
            throw new WeavingException(string.Format("An error occurred processing '{0}'. Error: {1}", Method.FullName, exception.Message));
        }
    }

    void InnerProcess()
    {
        body = Method.Body;
        body.SimplifyMacros();
        ilProcessor = body.GetILProcessor();
        
        var returnInstruction = FixReturns();

        var firstInstruction = FirstInstructionSkipCtor();

        var beforeReturn = Instruction.Create(OpCodes.Nop);
        ilProcessor.InsertBefore(returnInstruction, beforeReturn);

        InjectIlForFinaly(returnInstruction);

        var handler = new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = firstInstruction,
                TryEnd = beforeReturn,
                HandlerStart = beforeReturn,
                HandlerEnd = returnInstruction,
            };

        body.ExceptionHandlers.Add(handler);
        body.InitLocals = true;
        body.OptimizeMacros();
    }

    Instruction FirstInstructionSkipCtor()
    {
        if (Method.IsConstructor && !Method.IsStatic)
        {
            return body.Instructions.Skip(2).First();
        }
        return body.Instructions.First();
    }

    Instruction FixReturns()
    {
        if (Method.ReturnType == TypeSystem.Void)
        {
            var instructions = body.Instructions;
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
            var instructions = body.Instructions;
            var returnVariable = new VariableDefinition("methodTimerReturn", Method.ReturnType);
            body.Variables.Add(returnVariable);
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

    void InjectIlForFinaly(Instruction beforeThis)
    {

        var stopwatchVar = InjectStopwatch();

        var writeTimeIl = GetWriteTimeIL(stopwatchVar);


        foreach (var instruction in writeTimeIl)
        {
            ilProcessor.InsertBefore(beforeThis, instruction);
        }
        ilProcessor.InsertBefore(beforeThis, Instruction.Create(OpCodes.Endfinally));
    }

    List<Instruction> GetWriteTimeIL(VariableDefinition stopwatchVar)
    {
        if (InterceptorFinder.LogMethod == null)
        {
            string methodName;
            if (Method.IsConstructor)
            {
                methodName = string.Format("{0}{1} ", Method.DeclaringType.Name, Method.Name);
            }
            else
            {
                methodName = string.Format("{0}.{1} ", Method.DeclaringType.Name, Method.Name);
            }
            return new List<Instruction>
                {
                    Instruction.Create(OpCodes.Ldloc, stopwatchVar),
                    Instruction.Create(OpCodes.Call, ReferenceFinder.StopMethod),
                    Instruction.Create(OpCodes.Ldstr, methodName),
                    Instruction.Create(OpCodes.Ldloc, stopwatchVar),
                    Instruction.Create(OpCodes.Call, ReferenceFinder.ElapsedMilliseconds),
                    Instruction.Create(OpCodes.Box, TypeSystem.Int64),
                    Instruction.Create(OpCodes.Ldstr, "ms"),
                    Instruction.Create(OpCodes.Call, ReferenceFinder.ConcatMethod),
                    Instruction.Create(OpCodes.Call, ReferenceFinder.DebugWriteLineMethod),
                };
        }
        return new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldloc, stopwatchVar),
                Instruction.Create(OpCodes.Call, ReferenceFinder.StopMethod),
                Instruction.Create(OpCodes.Ldtoken, Method),
                Instruction.Create(OpCodes.Call, ReferenceFinder.GetMethodFromHandle),
                Instruction.Create(OpCodes.Ldloc, stopwatchVar),
                Instruction.Create(OpCodes.Call, ReferenceFinder.ElapsedMilliseconds),   
                Instruction.Create(OpCodes.Call, InterceptorFinder.LogMethod),   
            };
    }

    VariableDefinition InjectStopwatch()
    {
        var stopwatchVar = new VariableDefinition("methodTimerStopwatch", ReferenceFinder.StopwatchType);
        body.Variables.Add(stopwatchVar);


        body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, ReferenceFinder.StartNewMethod));
        body.Instructions.Insert(1, Instruction.Create(OpCodes.Stloc, stopwatchVar));
        return stopwatchVar;
    }
}