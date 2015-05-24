using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{

    
    public IEnumerable<Instruction> GetWriteTimeInstruction(VariableDefinition variableDefinition, MethodDefinition methodDefinition)
    {
        yield return Instruction.Create(OpCodes.Ldloc, variableDefinition);
        yield return Instruction.Create(OpCodes.Call, StopMethod);
        if (LogMethod == null)
        {
            yield return Instruction.Create(OpCodes.Ldstr, methodDefinition.MethodName());
            yield return Instruction.Create(OpCodes.Ldloc, variableDefinition);
            yield return Instruction.Create(OpCodes.Call, ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Box,ModuleDefinition.TypeSystem.Int64);
            yield return Instruction.Create(OpCodes.Ldstr, "ms");
            yield return Instruction.Create(OpCodes.Call, ConcatMethod);
            yield return Instruction.Create(OpCodes.Call, DebugWriteLineMethod);
        }
        else
        {
            yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition);
            yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition.DeclaringType);
            yield return Instruction.Create(OpCodes.Call, GetMethodFromHandle);
            yield return Instruction.Create(OpCodes.Ldloc, variableDefinition);
            yield return Instruction.Create(OpCodes.Call, ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Call, LogMethod);
        }
    }

    public VariableDefinition InjectStopwatch(MethodBody body, int index)
    {
        // inject as variable
        var stopwatchVar = new VariableDefinition("methodTimerStopwatch", StopwatchType);
        body.Variables.Add(stopwatchVar);
        body.Insert(index, new List<Instruction>(new[] {
            Instruction.Create(OpCodes.Call, StartNewMethod),
            Instruction.Create(OpCodes.Stloc, stopwatchVar)
        }));
        return stopwatchVar;
    }

}