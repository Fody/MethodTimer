using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public partial class MethodProcessor
{
    public ModuleWeaver ModuleWeaver;
    public MethodDefinition Method;
    MethodBody body;
    VariableDefinition startTimestampVar;
    VariableDefinition endTimestampVar;
    VariableDefinition durationTimestampVar;
    VariableDefinition durationTimespanVar;

    ParameterFormattingProcessor parameterFormattingProcessor = new();

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

        startTimestampVar = InjectStartTimestamp(body, indexOf);
        endTimestampVar = InjectEndTimestamp(body);
        durationTimestampVar = InjectDurationTimestamp(body);
        durationTimespanVar = InjectDurationTimespan(body);

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

                if (instruction.Operand is not MethodReference methodReference)
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
        foreach (var instruction in GetWriteTimeInstruction(startTimestampVar, endTimestampVar,
            durationTimestampVar, durationTimespanVar, Method))
        {
            body.InsertBefore(beforeThis, instruction);
        }

        body.InsertBefore(beforeThis, Instruction.Create(OpCodes.Endfinally));
    }

    public IEnumerable<Instruction> GetWriteTimeInstruction(VariableDefinition startTimestampVar,
        VariableDefinition endTimestampVar, VariableDefinition durationTimestampVar,
        VariableDefinition durationTimespanVar, MethodDefinition methodDefinition)
    {
        // endTimestamp = Stopwatch.GetTimestamp();
        yield return Instruction.Create(OpCodes.Call, ModuleWeaver.Stopwatch_GetTimestampMethod);
        yield return Instruction.Create(OpCodes.Stloc, endTimestampVar);

        // durationTimestamp = endTimestampVar - startTimestampVar;
        yield return Instruction.Create(OpCodes.Ldloc, endTimestampVar);
        yield return Instruction.Create(OpCodes.Ldloc, startTimestampVar);
        yield return Instruction.Create(OpCodes.Sub);
        yield return Instruction.Create(OpCodes.Stloc, durationTimestampVar);

        // durationTimespanVar = new TimeSpan((long)(MethodTimerHelper.TimestampToTicks * (double)durationTimestamp));

        yield return Instruction.Create(OpCodes.Ldloca, durationTimespanVar);
        yield return Instruction.Create(OpCodes.Ldsfld, ModuleWeaver.MethodTimerHelper_TimestampToTicks);
        yield return Instruction.Create(OpCodes.Ldloc, durationTimestampVar);
        yield return Instruction.Create(OpCodes.Conv_R8);
        yield return Instruction.Create(OpCodes.Mul);
        yield return Instruction.Create(OpCodes.Conv_I8);
        yield return Instruction.Create(OpCodes.Call, ModuleWeaver.TimeSpan_ConstructorMethod);

        var logWithMessageMethodUsingLong = ModuleWeaver.LogWithMessageMethodUsingLong;
        var logWithMessageMethodUsingTimeSpan = ModuleWeaver.LogWithMessageMethodUsingTimeSpan;

        var logMethodUsingLong = ModuleWeaver.LogMethodUsingLong;
        var logMethodUsingTimeSpan = ModuleWeaver.LogMethodUsingTimeSpan;

        if (logWithMessageMethodUsingLong is null && logWithMessageMethodUsingTimeSpan is null)
        {
            if (logMethodUsingLong is null && logMethodUsingTimeSpan is null)
            {
                // var elapsedMillisecondsVariable = (long)durationTimespanVar.TotalMilliseconds;
                // Trace.WriteLine(string.Concat(methodName, elapsedMillisecondsVariable.ToString(), "ms"))

                var elapsedMillisecondsVariable = new VariableDefinition(ModuleWeaver.TypeSystem.Int64Reference);
                methodDefinition.Body.Variables.Add(elapsedMillisecondsVariable);
                yield return Instruction.Create(OpCodes.Ldstr, methodDefinition.MethodName());
                yield return Instruction.Create(OpCodes.Ldloca, durationTimespanVar);
                yield return Instruction.Create(OpCodes.Call, ModuleWeaver.TimeSpan_TotalMillisecondsMethod);
                yield return Instruction.Create(OpCodes.Conv_I8);
                yield return Instruction.Create(OpCodes.Stloc, elapsedMillisecondsVariable);
                yield return Instruction.Create(OpCodes.Ldloca, elapsedMillisecondsVariable);
                yield return Instruction.Create(OpCodes.Call, ModuleWeaver.Int64ToString);
                yield return Instruction.Create(OpCodes.Ldstr, "ms");
                yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ConcatMethod);
                yield return Instruction.Create(OpCodes.Call, ModuleWeaver.TraceWriteLineMethod);

                yield break;
            }

            // push the MethodBase
            yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition);
            yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition.DeclaringType);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.GetMethodFromHandle);

            if (logMethodUsingTimeSpan is not null)
            {
                // logMethod(MethodBase, TimeSpan)
                yield return Instruction.Create(OpCodes.Ldloc, durationTimespanVar);
                yield return Instruction.Create(OpCodes.Call, logMethodUsingTimeSpan);
                yield break;
            }

            // logMethod(MethodBase, long)
            yield return Instruction.Create(OpCodes.Ldloca, durationTimespanVar);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.TimeSpan_TotalMillisecondsMethod);
            yield return Instruction.Create(OpCodes.Conv_I8);
            yield return Instruction.Create(OpCodes.Call, logMethodUsingLong);
            yield break;
        }

        var formattedVariableDefinition = new VariableDefinition(ModuleWeaver.TypeSystem.StringReference);
        methodDefinition.Body.Variables.Add(formattedVariableDefinition);

        // formattedVariableDefinition = <message to log>
        foreach (var instruction in ProcessTimeAttribute(methodDefinition, formattedVariableDefinition))
        {
            yield return instruction;
        }

        // push the MethodBase
        yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition);
        yield return Instruction.Create(OpCodes.Ldtoken, methodDefinition.DeclaringType);
        yield return Instruction.Create(OpCodes.Call, ModuleWeaver.GetMethodFromHandle);

        if (logWithMessageMethodUsingTimeSpan is null)
        {
            // logMethod(MethodBase, long, string)
            yield return Instruction.Create(OpCodes.Ldloca, durationTimespanVar);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.TimeSpan_TotalMillisecondsMethod);
            yield return Instruction.Create(OpCodes.Conv_I8);
            yield return Instruction.Create(OpCodes.Ldloc, formattedVariableDefinition);
            yield return Instruction.Create(OpCodes.Call, logWithMessageMethodUsingLong);
            yield break;
        }

        // logMethod(MethodBase, TimeSpan, string)
        yield return Instruction.Create(OpCodes.Ldloc, durationTimespanVar);
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
            // Load a null string
            yield return Instruction.Create(OpCodes.Ldnull);
        }
        else
        {
            var info = parameterFormattingProcessor.ParseParameterFormatting(value);

            yield return Instruction.Create(OpCodes.Ldstr, info.Format);
            yield return Instruction.Create(OpCodes.Ldc_I4, info.ParameterNames.Count);
            yield return Instruction.Create(OpCodes.Newarr, ModuleWeaver.TypeSystem.ObjectReference);

            for (var i = 0; i < info.ParameterNames.Count; i++)
            {
                // Note: no need to validate, already done in AssemblyProcessor::ProcessMethod
                var parameterName = info.ParameterNames[i];

                yield return Instruction.Create(OpCodes.Dup);
                yield return Instruction.Create(OpCodes.Ldc_I4, i);

                if (string.Equals(parameterName, "this"))
                {
                    yield return Instruction.Create(OpCodes.Ldarg_0);
                }
                else
                {
                    var parameter = methodDefinition.Parameters.First(p => p.Name.Equals(parameterName));
                    yield return Instruction.Create(OpCodes.Ldarg, parameter);

                    if (parameter.ParameterType.IsBoxingRequired(ModuleWeaver.TypeSystem.ObjectReference))
                    {
                        yield return Instruction.Create(OpCodes.Box, ModuleWeaver.ModuleDefinition.ImportReference(parameter.ParameterType));
                    }
                }

                yield return Instruction.Create(OpCodes.Stelem_Ref);
            }

            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.StringFormatWithArray);
        }

        yield return Instruction.Create(OpCodes.Stloc, formattedVariableDefinition);
    }
}
