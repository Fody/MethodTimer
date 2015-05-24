using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

public class MethodProcessor
{
    public ModuleWeaver ModuleWeaver;
    public MethodDefinition Method;
    MethodBody body;
    VariableDefinition stopwatchVar;
    List<Instruction> returnPoints;

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
            returnPoints = GetSyncReturnPoints(body.Instructions);

        body.SimplifyMacros();
        stopwatchVar = ModuleWeaver.InjectStopwatch(body);
        HandleReturns();
        body.InitLocals = true;
        body.OptimizeMacros();
    }

    static List<Instruction> GetSyncReturnPoints(Collection<Instruction> instructions)
    {
        var returnPoints = instructions.Where(x => x.OpCode == OpCodes.Ret).ToList();
        var last = instructions.Last();
        if (last.OpCode == OpCodes.Rethrow || last.OpCode == OpCodes.Throw)
        {
            returnPoints.Add(last);
        }
        return returnPoints;
    }

    void HandleReturns()
    {
        foreach (var returnPoint in returnPoints)
        {
            FixReturn(returnPoint);
        }
    }

    void FixReturn(Instruction returnPoint)
    {
        var opCode = returnPoint.OpCode;
        var operand = returnPoint.Operand as Instruction;

        returnPoint.OpCode = OpCodes.Nop;
        returnPoint.Operand = null;

        var instructions = body.Instructions;
        var indexOf = instructions.IndexOf(returnPoint);
        foreach (var instruction in ModuleWeaver.GetWriteTimeInstruction(stopwatchVar,Method))
        {
            indexOf++;
            instructions.Insert(indexOf, instruction);
        }

        indexOf++;

        if ((opCode == OpCodes.Leave) || (opCode == OpCodes.Leave_S))
        {
            instructions.Insert(indexOf, Instruction.Create(opCode, operand));
        }
        else
        {
            instructions.Insert(indexOf, Instruction.Create(opCode));
        }
    }
}

