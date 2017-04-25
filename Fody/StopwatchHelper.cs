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

        var logWithMessageMethod = LogWithMessageMethod;
        var logMethod = LogMethod;

        if (logWithMessageMethod != null)
        {
            var formattedVariableDefinition = new VariableDefinition(ModuleDefinition.TypeSystem.String);
            methodDefinition.Body.Variables.Add(formattedVariableDefinition);

            // Load everything for a string format
            var timeAttribute = methodDefinition.GetTimeAttribute();
            if (timeAttribute != null)
            {
                var value = timeAttribute.ConstructorArguments.FirstOrDefault().Value as string;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var info = parameterFormattingProcessor.ParseParameterFormatting(value);

                    yield return Instruction.Create(OpCodes.Ldstr, info.Format);
                    yield return Instruction.Create(OpCodes.Ldc_I4, info.ParameterNames.Count);
                    yield return Instruction.Create(OpCodes.Newarr, ModuleDefinition.TypeSystem.Object);

                    for (var i = 0; i < info.ParameterNames.Count; i++)
                    {
                        var parameter = methodDefinition.Parameters.First(x => x.Name.Equals(info.ParameterNames[i]));

                        yield return Instruction.Create(OpCodes.Dup);
                        yield return Instruction.Create(OpCodes.Ldc_I4, i);
                        yield return Instruction.Create(OpCodes.Ldarg, parameter);

                        if (parameter.ParameterType.IsBoxingRequired(ModuleDefinition.TypeSystem.Object))
                        {
                            yield return Instruction.Create(OpCodes.Box,
                                ModuleDefinition.ImportReference(parameter.ParameterType));
                        }

                        yield return Instruction.Create(OpCodes.Stelem_Ref);
                    }

                    yield return Instruction.Create(OpCodes.Call, StringFormatWithArray);
                }
                else
                {
                    // Load null a string
                    yield return Instruction.Create(OpCodes.Ldnull);
                }

                yield return Instruction.Create(OpCodes.Stloc, formattedVariableDefinition);
            }

            // Handle call to log method
            yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition);
            yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition.DeclaringType);
            yield return Instruction.Create(OpCodes.Call, GetMethodFromHandle);
            yield return Instruction.Create(OpCodes.Ldloc, stopwatchVariableDefinition);
            yield return Instruction.Create(OpCodes.Call, ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Ldloc, formattedVariableDefinition);
            yield return Instruction.Create(OpCodes.Call, logWithMessageMethod);
        }
        else if (logMethod != null)
        {
            yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition);
            yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition.DeclaringType);
            yield return Instruction.Create(OpCodes.Call, GetMethodFromHandle);
            yield return Instruction.Create(OpCodes.Ldloc, stopwatchVariableDefinition);
            yield return Instruction.Create(OpCodes.Call, ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Call, logMethod);
        }
        else
        {
            yield return Instruction.Create(OpCodes.Ldstr, methodDefinition.MethodName());
            yield return Instruction.Create(OpCodes.Ldloc, stopwatchVariableDefinition);
            yield return Instruction.Create(OpCodes.Call, ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Box, ModuleDefinition.TypeSystem.Int64);
            yield return Instruction.Create(OpCodes.Ldstr, "ms");
            yield return Instruction.Create(OpCodes.Call, ConcatMethod);
            yield return Instruction.Create(OpCodes.Call, DebugWriteLineMethod);
        }
    }

    public VariableDefinition InjectStopwatch(MethodBody body, int index)
    {
        // inject as variable
        var stopwatchVar = new VariableDefinition(StopwatchType);
        body.Variables.Add(stopwatchVar);
        body.Insert(index, new List<Instruction>(new[] {
            Instruction.Create(OpCodes.Call, StartNewMethod),
            Instruction.Create(OpCodes.Stloc, stopwatchVar)
        }));
        return stopwatchVar;
    }

}