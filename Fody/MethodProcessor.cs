using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public class MethodProcessor
{
    public ModuleWeaver ModuleWeaver;
    public TypeSystem TypeSystem;
    public MethodDefinition Method;
    MethodBody body;
    ILProcessor ilProcessor;
    MethodDefinition originalMethod;

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
        originalMethod = Method;
        var asyncAttribute = Method.CustomAttributes.FirstOrDefault(_ => _.AttributeType.Name == "AsyncStateMachineAttribute");
        if (asyncAttribute != null)
        {
            var fullName = Method.FullName;
            var customAttributeArgument = asyncAttribute.ConstructorArguments.First();
            var typeReference = (TypeReference) customAttributeArgument.Value;
            var typeDefinition = typeReference.Resolve();
            Method = typeDefinition.Methods.FirstOrDefault(_ => _.Name == "MoveNext");
            if (Method == null)
            {
                var format = string.Format("Could not weave '{0}' it is possible you are using a newver version of .net that has not been catered for. Please raise an issue.", fullName);
                ModuleWeaver.LogWarning(format);
                return;
            }
        }
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
        var instructions = body.Instructions;
        if (Method.ReturnType == TypeSystem.Void)
        {
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
        foreach (var instruction in GetWriteTimeIL())
        {
            ilProcessor.InsertBefore(beforeThis, instruction);
        }
        ilProcessor.InsertBefore(beforeThis, Instruction.Create(OpCodes.Endfinally));
    }

    List<Instruction> GetWriteTimeIL()
    {
        var stopwatchVar = InjectStopwatch();

        if (ModuleWeaver.LogMethod == null)
        {
            string methodName;
            if (Method.IsConstructor)
            {
                methodName = string.Format("{0}{1} ", originalMethod.DeclaringType.Name, originalMethod.Name);
            }
            else
            {
                methodName = string.Format("{0}.{1} ", originalMethod.DeclaringType.Name, originalMethod.Name);
            }
            return new List<Instruction>
                {
                    Instruction.Create(OpCodes.Ldloc, stopwatchVar),
                    Instruction.Create(OpCodes.Call, ModuleWeaver.StopMethod),
                    Instruction.Create(OpCodes.Ldstr, methodName),
                    Instruction.Create(OpCodes.Ldloc, stopwatchVar),
                    Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds),
                    Instruction.Create(OpCodes.Box, TypeSystem.Int64),
                    Instruction.Create(OpCodes.Ldstr, "ms"),
                    Instruction.Create(OpCodes.Call, ModuleWeaver.ConcatMethod),
                    Instruction.Create(OpCodes.Call, ModuleWeaver.DebugWriteLineMethod),
                };
        }
        return new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldloc, stopwatchVar),
                Instruction.Create(OpCodes.Call, ModuleWeaver.StopMethod),
                Instruction.Create(OpCodes.Ldtoken, originalMethod),
                Instruction.Create(OpCodes.Call, ModuleWeaver.GetMethodFromHandle),
                Instruction.Create(OpCodes.Ldloc, stopwatchVar),
                Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds),
                Instruction.Create(OpCodes.Call, ModuleWeaver.LogMethod),
            };
    }

    VariableDefinition InjectStopwatch()
    {
        var stopwatchVar = new VariableDefinition("methodTimerStopwatch", ModuleWeaver.StopwatchType);
        body.Variables.Add(stopwatchVar);


        body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, ModuleWeaver.StartNewMethod));
        body.Instructions.Insert(1, Instruction.Create(OpCodes.Stloc, stopwatchVar));
        return stopwatchVar;
    }
}