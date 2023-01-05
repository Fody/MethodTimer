using System;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public class MethodProcessor
{
    public ModuleWeaver ModuleWeaver;
    public MethodDefinition Method;
    MethodBody body;
    VariableDefinition stopwatchVar;

    public void Process()
    {
        try
        {
            InnerProcess();
        }
        catch (Exception exception)
        {
            throw new WeavingException($"An error occurred processing '{Method.FullName}'. Error: {exception.Message}");
        }
    }

    void InnerProcess()
    {
        body = Method.Body;
        body.SimplifyMacros();

        var firstInstruction = FirstInstructionSkipCtor();

        var indexOf = body.Instructions.IndexOf(firstInstruction);

        var returnInstruction = FixReturns();

        stopwatchVar = ModuleWeaver.InjectStopwatch(body, indexOf);

        var beforeReturn = Instruction.Create(OpCodes.Nop);
        body.InsertBefore(returnInstruction, beforeReturn);

        InjectIlForFinally(returnInstruction);

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
        if (Method.IsInstanceConstructor())
        {
            foreach (var instruction in body.Instructions)
            {
                if (instruction.OpCode != OpCodes.Call)
                {
                    continue;
                }

                if (!(instruction.Operand is MethodReference methodReference))
                {
                    continue;
                }

                if (methodReference.Name != ".ctor")
                {
                    continue;
                }

                if (methodReference.DeclaringType != Method.DeclaringType.BaseType)
                {
                    continue;
                }

                return instruction.Next;
            }
        }
        return body.Instructions.First();
    }

    Instruction FixReturns()
    {
        var instructions = body.Instructions;
        if (Method.ReturnType.FullName == ModuleWeaver.TypeSystem.VoidReference.FullName)
        {
            var lastRet = Instruction.Create(OpCodes.Ret);

            foreach (var instruction in instructions)
            {
                if (instruction.OpCode == OpCodes.Ret)
                {
                    instruction.OpCode = OpCodes.Leave;
                    instruction.Operand = lastRet;
                }
            }
            instructions.Add(lastRet);
            return lastRet;
        }
        var returnVariable = new VariableDefinition(Method.ReturnType);
        body.Variables.Add(returnVariable);
        var lastLd = Instruction.Create(OpCodes.Ldloc, returnVariable);
        for (var index = 0; index < instructions.Count; index++)
        {
            var instruction = instructions[index];
            if (instruction.OpCode == OpCodes.Ret)
            {
                instruction.OpCode = OpCodes.Stloc;
                instruction.Operand = returnVariable;
                index++;
                instructions.Insert(index, Instruction.Create(OpCodes.Leave, lastLd));
            }
        }
        instructions.Add(lastLd);
        instructions.Add(Instruction.Create(OpCodes.Ret));
        return lastLd;
    }

    void InjectIlForFinally(Instruction beforeThis)
    {
        foreach (var instruction in ModuleWeaver.GetWriteTimeInstruction(stopwatchVar, Method))
        {
            body.InsertBefore(beforeThis, instruction);
        }

        body.InsertBefore(beforeThis, Instruction.Create(OpCodes.Endfinally));
    }
}
