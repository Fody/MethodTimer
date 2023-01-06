using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    public IEnumerable<Instruction> GetWriteTimeInstruction(VariableDefinition stopwatchVariableDefinition, MethodDefinition methodDefinition)
    {
        yield return Instruction.Create(OpCodes.Ldloc, stopwatchVariableDefinition);
        yield return Instruction.Create(OpCodes.Call, StopMethod);

        var logWithMessageMethodUsingLong = LogWithMessageMethodUsingLong;
        var logWithMessageMethodUsingTimeSpan = LogWithMessageMethodUsingTimeSpan;

        var logMethodUsingLong = LogMethodUsingLong;
        var logMethodUsingTimeSpan = LogMethodUsingTimeSpan;

        if (logWithMessageMethodUsingLong is null && logWithMessageMethodUsingTimeSpan is null)
        {
            if (logMethodUsingLong is null && logMethodUsingTimeSpan is null)
            {
                var elapsedMillisecondsVariable = new VariableDefinition(TypeSystem.Int64Reference);
                methodDefinition.Body.Variables.Add(elapsedMillisecondsVariable);
                yield return Instruction.Create(OpCodes.Ldstr, methodDefinition.MethodName());
                yield return Instruction.Create(OpCodes.Ldloc, stopwatchVariableDefinition);
                yield return Instruction.Create(OpCodes.Call, ElapsedMilliseconds);
                yield return Instruction.Create(OpCodes.Stloc, elapsedMillisecondsVariable);
                yield return Instruction.Create(OpCodes.Ldloca, elapsedMillisecondsVariable);
                yield return Instruction.Create(OpCodes.Call, Int64ToString);
                yield return Instruction.Create(OpCodes.Ldstr, "ms");
                yield return Instruction.Create(OpCodes.Call, ConcatMethod);
                yield return Instruction.Create(OpCodes.Call, TraceWriteLineMethod);

                yield break;
            }

            yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition);
            yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition.DeclaringType);
            yield return Instruction.Create(OpCodes.Call, GetMethodFromHandle);
            yield return Instruction.Create(OpCodes.Ldloc, stopwatchVariableDefinition);

            if (logMethodUsingTimeSpan != null)
            {
                yield return Instruction.Create(OpCodes.Call, Elapsed);
                yield return Instruction.Create(OpCodes.Call, logMethodUsingTimeSpan);
                yield break;
            }

            yield return Instruction.Create(OpCodes.Call, ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Call, logMethodUsingLong);
            yield break;
        }

        var formattedVariableDefinition = new VariableDefinition(TypeSystem.StringReference);
        methodDefinition.Body.Variables.Add(formattedVariableDefinition);

        foreach (var instruction in ProcessTimeAttribute(methodDefinition, formattedVariableDefinition))
        {
            yield return instruction;
        }

        // Handle call to log method
        yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition);
        yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition.DeclaringType);
        yield return Instruction.Create(OpCodes.Call, GetMethodFromHandle);
        yield return Instruction.Create(OpCodes.Ldloc, stopwatchVariableDefinition);

        if (logWithMessageMethodUsingTimeSpan is null)
        {
            yield return Instruction.Create(OpCodes.Call, ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Ldloc, formattedVariableDefinition);
            yield return Instruction.Create(OpCodes.Call, logWithMessageMethodUsingLong);
            yield break;
        }

        yield return Instruction.Create(OpCodes.Call, Elapsed);
        yield return Instruction.Create(OpCodes.Ldloc, formattedVariableDefinition);
        yield return Instruction.Create(OpCodes.Call, logWithMessageMethodUsingTimeSpan);
    }

    IEnumerable<Instruction> ProcessTimeAttribute(MethodDefinition methodDefinition, VariableDefinition formattedVariableDefinition)
    {
        // Load everything for a string format
        var timeAttribute = methodDefinition.GetTimeAttribute();
        if (timeAttribute == null)
        {
            yield break;
        }

        var value = timeAttribute.ConstructorArguments.FirstOrDefault().Value as string;
        if (string.IsNullOrWhiteSpace(value))
        {
            // Load null a string
            yield return Instruction.Create(OpCodes.Ldnull);
        }
        else
        {
            var info = parameterFormattingProcessor.ParseParameterFormatting(value);

            yield return Instruction.Create(OpCodes.Ldstr, info.Format);
            yield return Instruction.Create(OpCodes.Ldc_I4, info.ParameterNames.Count);
            yield return Instruction.Create(OpCodes.Newarr, TypeSystem.ObjectReference);

            for (var i = 0; i < info.ParameterNames.Count; i++)
            {
                // Note: no need to validate, already done in AssemblyProcessor::ProcessMethod
                var parameterName = info.ParameterNames[i];

                yield return Instruction.Create(OpCodes.Dup);
                yield return Instruction.Create(OpCodes.Ldc_I4, i);

                if (string.Equals(parameterName, "this"))
                {
                    // IL_0028: ldarg.0
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                }
                else
                {
                    var parameter = methodDefinition.Parameters.First(x => x.Name.Equals(parameterName));
                    yield return Instruction.Create(OpCodes.Ldarg, parameter);

                    if (parameter.ParameterType.IsBoxingRequired(TypeSystem.ObjectReference))
                    {
                        yield return Instruction.Create(OpCodes.Box, ModuleDefinition.ImportReference(parameter.ParameterType));
                    }
                }

                yield return Instruction.Create(OpCodes.Stelem_Ref);
            }

            yield return Instruction.Create(OpCodes.Call, StringFormatWithArray);
        }

        yield return Instruction.Create(OpCodes.Stloc, formattedVariableDefinition);
    }

    public VariableDefinition InjectStopwatch(MethodBody body, int index)
    {
        // inject as variable
        var stopwatchVar = new VariableDefinition(StopwatchType);
        body.Variables.Add(stopwatchVar);
        body.Insert(index, new List<Instruction>(
            new[]
            {
                Instruction.Create(OpCodes.Call, StartNewMethod),
                Instruction.Create(OpCodes.Stloc, stopwatchVar)
            }));
        return stopwatchVar;
    }
}