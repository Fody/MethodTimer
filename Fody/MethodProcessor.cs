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

    public void InnerProcess()
    {
        body = Method.Body;
        body.SimplifyMacros();
        InjectStopwatch();
        HandleReturns();
        body.InitLocals = true;
        body.OptimizeMacros();
    }

    void HandleReturns()
    {
        var returnPoints = body.Instructions.Where(x => x.OpCode == OpCodes.Ret).ToList();

        foreach (var returnPoint in returnPoints)
        {
            FixReturn(returnPoint);
        }
        var last = body.Instructions.Last();
        if (last.OpCode == OpCodes.Rethrow || last.OpCode == OpCodes.Throw)
        {
            FixReturn(last);
        }
    }

    void FixReturn(Instruction returnPoint)
    {
        var opCode = returnPoint.OpCode;
        returnPoint.OpCode = OpCodes.Nop;
        var indexOf = body.Instructions.IndexOf(returnPoint);
        foreach (var instruction in GetWriteTimeIL())
        {
            indexOf++;
            body.Instructions.Insert(indexOf, instruction);
        }
        indexOf++;
        body.Instructions.Insert(indexOf, Instruction.Create(opCode));
    }


    IEnumerable<Instruction> GetWriteTimeIL()
    {
        yield return Instruction.Create(OpCodes.Ldloc, stopwatchVar);
        yield return Instruction.Create(OpCodes.Call, ModuleWeaver.StopMethod);
        if (ModuleWeaver.LogMethod == null)
        {
            yield return Instruction.Create(OpCodes.Ldstr, Method.MethodName());
            yield return Instruction.Create(OpCodes.Ldloc, stopwatchVar);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Box, TypeSystem.Int64);
            yield return Instruction.Create(OpCodes.Ldstr, "ms");
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ConcatMethod);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.DebugWriteLineMethod);
        }
        else
        {
            yield return Instruction.Create(OpCodes.Ldtoken, Method);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.GetMethodFromHandle);
            yield return Instruction.Create(OpCodes.Ldloc, stopwatchVar);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.LogMethod);
        }
    }


    void InjectStopwatch()
    {
        stopwatchVar = new VariableDefinition("methodTimerStopwatch", ModuleWeaver.StopwatchType);
        body.Variables.Add(stopwatchVar);
        body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, ModuleWeaver.StartNewMethod));
        body.Instructions.Insert(1, Instruction.Create(OpCodes.Stloc, stopwatchVar));
    }
}