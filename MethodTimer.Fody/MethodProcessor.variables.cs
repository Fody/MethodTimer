using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public partial class MethodProcessor
{
    public VariableDefinition InjectStartTimestamp(MethodBody body, int index)
    {
        // inject as variable
        var startTimestampVariable = new VariableDefinition(ModuleWeaver.TypeSystem.Int64Reference);
        body.Variables.Add(startTimestampVariable);
        body.Insert(index, new List<Instruction>(
            new[]
            {
                Instruction.Create(OpCodes.Call, ModuleWeaver.Stopwatch_GetTimestampMethod),
                Instruction.Create(OpCodes.Stloc, startTimestampVariable)
            }));
        return startTimestampVariable;
    }

    public VariableDefinition InjectEndTimestamp(MethodBody body, int index)
    {
        // inject as variable
        var endTimestampVariable = new VariableDefinition(ModuleWeaver.TypeSystem.Int64Reference);
        body.Variables.Add(endTimestampVariable);

        return endTimestampVariable;
    }

    public VariableDefinition InjectDurationTimestamp(MethodBody body, int index)
    {
        // inject as variable
        var durationTimestampVariable = new VariableDefinition(ModuleWeaver.TypeSystem.Int64Reference);
        body.Variables.Add(durationTimestampVariable);

        return durationTimestampVariable;
    }

    public VariableDefinition InjectDurationTimespan(MethodBody body, int index)
    {
        // inject as variable
        var durationTimespan = new VariableDefinition(ModuleWeaver.TimeSpanType);
        body.Variables.Add(durationTimespan);

        return durationTimespan;
    }
}