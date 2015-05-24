using System;
using System.Linq;
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
            throw new WeavingException(string.Format("An error occurred processing '{0}'. Error: {1}", Method.FullName, exception.Message));
        }
    }

    void InnerProcess()
    {
        body = Method.Body;
        body.SimplifyMacros();

        var returnInstruction = FixReturns();

        var firstInstruction = FirstInstructionSkipCtor();

        stopwatchVar = ModuleWeaver.InjectStopwatch(body);

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
            return body.Instructions.Skip(2).First();
        }
        return body.Instructions.First();
    }


    Instruction FixReturns()
    {
        var instructions = body.Instructions;
        if (Method.ReturnType == ModuleWeaver.ModuleDefinition.TypeSystem.Void)
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
                instruction.OpCode = OpCodes.Leave;
                instruction.Operand = lastLd;
                instructions.Insert(index, Instruction.Create(OpCodes.Stloc, returnVariable));
                index++;
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
