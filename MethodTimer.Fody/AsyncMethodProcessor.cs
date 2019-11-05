using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public class AsyncMethodProcessor
{
    public ModuleWeaver ModuleWeaver;
    public MethodDefinition Method;
    MethodBody body;
    FieldDefinition stopwatchFieldDefinition;
    FieldReference stopwatchFieldReference;
    FieldDefinition stateFieldDefinition;
    FieldReference stateFieldReference;
    TypeDefinition stateMachineTypeDefinition;
    TypeReference stateMachineTypeReference;
    MethodDefinition stopStopwatchMethod;
    ParameterFormattingProcessor parameterFormattingProcessor = new ParameterFormattingProcessor();

    public void Process()
    {
        try
        {
            InnerProcess();
        }
        catch (Exception exception)
        {
            throw new Exception($"An error occurred processing '{Method.FullName}'. Error: {exception.Message}", exception);
        }
    }

    void InnerProcess()
    {
        var asyncAttribute = Method.GetAsyncStateMachineAttribute();
        stateMachineTypeDefinition = asyncAttribute.ConstructorArguments
            .Select(ctor => (TypeDefinition)ctor.Value)
            .Single();

        if (stateMachineTypeDefinition.HasGenericParameters)
        {
            var genericStateMachineType = new GenericInstanceType(stateMachineTypeDefinition);

            foreach (var genericParameter in stateMachineTypeDefinition.GenericParameters)
            {
                genericStateMachineType.GenericArguments.Add(genericParameter);
            }

            stateMachineTypeReference = genericStateMachineType;
        }
        else
        {
            stateMachineTypeReference = stateMachineTypeDefinition;
        }

        var moveNextMethod = stateMachineTypeDefinition.Methods
            .Single(x => x.Name == "MoveNext");
        body = moveNextMethod.Body;

        body.SimplifyMacros();

        // First, fall back to old mechanism
        int index;

        // Check roslyn usage
        var firstStateUsage = (from instruction in body.Instructions
                               let fieldReference = instruction.Operand as FieldReference
                               where instruction.OpCode == OpCodes.Ldfld &&
                                     fieldReference != null &&
                                     fieldReference.Name.EndsWith("__state")
                               select instruction).FirstOrDefault();
        if (firstStateUsage is null)
        {
            // Probably compiled without roslyn, inject at first line
            index = 0;
        }
        else
        {
            // Initial code looks like this (hence the -1):
            //
            // <== this is where we want to start the stopwatch
            // ldarg.0
            // ldfld __state
            // stloc.0
            // ldloc.0
            index = body.Instructions.IndexOf(firstStateUsage) - 1;
        }

        stateFieldDefinition = (from x in stateMachineTypeDefinition.Fields
                      where x.Name.EndsWith("__state")
                      select x).First();

        stateFieldReference = new FieldReference(stateFieldDefinition.Name, stateFieldDefinition.FieldType, stateMachineTypeReference);

        InjectStopwatchStart(index, body.Instructions[index]);
        InjectStopwatchStopMethod();
        InjectStopwatchStopCalls();

        body.InitLocals = true;
        body.OptimizeMacros();
    }

    Instruction FixReturns()
    {
        var instructions = body.Instructions;

        // We inject both a nop and return. This allows us to inject between the nop
        // and the return
        var nop = Instruction.Create(OpCodes.Nop);
        var lastRet = Instruction.Create(OpCodes.Ret);

        foreach (var instruction in instructions)
        {
            if (instruction.OpCode == OpCodes.Ret)
            {
                instruction.OpCode = OpCodes.Leave;
                instruction.Operand = nop;
            }
        }

        instructions.Add(nop);
        instructions.Add(lastRet);

        return lastRet;
    }

    void InjectStopwatchStart(int index, Instruction nextInstruction)
    {
        var boolVariable = new VariableDefinition(ModuleWeaver.BooleanType.Resolve());
        body.Variables.Add(boolVariable);

        stopwatchFieldDefinition = new FieldDefinition("methodTimerStopwatch", new FieldAttributes(), ModuleWeaver.StopwatchType);
        stateMachineTypeDefinition.Fields.Add(stopwatchFieldDefinition);

        stopwatchFieldReference = new FieldReference(stopwatchFieldDefinition.Name, stopwatchFieldDefinition.FieldType, stateMachineTypeReference);

        body.Insert(index, new[]
        {
            // This code looks like this:
            // if (_stopwatch is null)
            // {
            //    _stopwatch = Stopwatch.StartNew();
            // }

            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, stopwatchFieldReference),
            Instruction.Create(OpCodes.Ldnull),
            Instruction.Create(OpCodes.Ceq),
            Instruction.Create(OpCodes.Stloc, boolVariable),
            Instruction.Create(OpCodes.Ldloc, boolVariable),
            Instruction.Create(OpCodes.Brfalse_S, nextInstruction),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, ModuleWeaver.StartNewMethod),
            Instruction.Create(OpCodes.Stfld, stopwatchFieldReference)
        });
    }

    void InjectStopwatchStopMethod()
    {
        if (stopStopwatchMethod != null)
        {
            return;
        }

        var method = new MethodDefinition("StopMethodTimerStopwatch", MethodAttributes.Private, ModuleWeaver.VoidType);

        var methodBody = method.Body;
        methodBody.SimplifyMacros();

        var stopwatchInstructions = GetWriteTimeInstruction().ToList();

        foreach (var instruction in stopwatchInstructions)
        {
            methodBody.Instructions.Add(instruction);
        }

        methodBody.InitLocals = true;
        methodBody.OptimizeMacros();

        stateMachineTypeDefinition.Methods.Add(method);

        stopStopwatchMethod = method;
    }

    void InjectStopwatchStopCalls()
    {
        // There are 2 locations where the stopwatch logic should be injected:
        // 1: just before the ::SetException
        // 2: end of the method (which is not executed after calling ::SetException) or just after SetResult

        var stopStopwatchMethodReference = new MethodReference(stopStopwatchMethod.Name, stopStopwatchMethod.ReturnType, stateMachineTypeReference)
        {
            HasThis = stopStopwatchMethod.HasThis,
            ExplicitThis = stopStopwatchMethod.ExplicitThis,
            CallingConvention = stopStopwatchMethod.CallingConvention
        };

        var stopwatchInstructions = new List<Instruction>(new[]
        {
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, stopStopwatchMethodReference)
        });

        var returnInstruction = FixReturns();
        var endInstruction = returnInstruction;

        // 1: SetException: We will search for the last known catch block, and implement a finally there with the stopwatch code
        var exceptionHandler = (from handler in body.ExceptionHandlers
                                orderby handler.HandlerEnd.Offset descending
                                select handler).FirstOrDefault();
        if (exceptionHandler != null)
        {
            var catchStartIndex = body.Instructions.IndexOf(exceptionHandler.HandlerStart);
            var catchEndIndex = body.Instructions.IndexOf(exceptionHandler.HandlerEnd);

            for (var i = catchEndIndex; i >= catchStartIndex; i--)
            {
                if (body.Instructions[i].Operand is MethodReference methodReference &&
                    methodReference.Name == "SetException")
                {
                    // Insert before
                    for (var j = 0; j < stopwatchInstructions.Count; j++)
                    {
                        body.Instructions.Insert(i + j, stopwatchInstructions[j]);
                    }
                    break;
                }
            }
        }

        // 2: end of the method (either SetResult or end of the method)
        for (var i = body.Instructions.Count - 1; i >= 0; i--)
        {
            if (body.Instructions[i].Operand is MethodReference methodReference &&
                methodReference.Name == "SetResult")
            {
                // Next index, we want this to appear *after* the SetResult call
                endInstruction = body.Instructions[i + 1];
                break;
            }
        }

        var startIndex = body.Instructions.IndexOf(endInstruction);

        for (var i = 0; i < stopwatchInstructions.Count; i++)
        {
            body.Instructions.Insert(startIndex++, stopwatchInstructions[i]);
        }
    }

    IEnumerable<Instruction> GetWriteTimeInstruction()
    {
        var stopwatchRunningCheck = Instruction.Create(OpCodes.Ldarg_0);
        var startOfRealMethod = Instruction.Create(OpCodes.Ldarg_0);

        // Check if state machine is completed (state == -2)
        yield return Instruction.Create(OpCodes.Ldarg_0);
        yield return Instruction.Create(OpCodes.Ldfld, stateFieldReference);
        yield return Instruction.Create(OpCodes.Ldc_I4, -2);
        yield return Instruction.Create(OpCodes.Beq_S, stopwatchRunningCheck);
        yield return Instruction.Create(OpCodes.Ret);

        // Check if stopwatch is actually running
        yield return stopwatchRunningCheck;
        yield return Instruction.Create(OpCodes.Ldfld, stopwatchFieldReference);
        yield return Instruction.Create(OpCodes.Callvirt, ModuleWeaver.IsRunning);
        yield return Instruction.Create(OpCodes.Ldc_I4_0);
        yield return Instruction.Create(OpCodes.Ceq);
        yield return Instruction.Create(OpCodes.Brfalse_S, startOfRealMethod);
        yield return Instruction.Create(OpCodes.Ret);

        yield return startOfRealMethod; // Ldarg_0
        yield return Instruction.Create(OpCodes.Ldfld, stopwatchFieldReference);
        yield return Instruction.Create(OpCodes.Call, ModuleWeaver.StopMethod);

        var logWithMessageMethodUsingLong = ModuleWeaver.LogWithMessageMethodUsingLong;
        var logWithMessageMethodUsingTimeSpan = ModuleWeaver.LogWithMessageMethodUsingTimeSpan;

        var logMethodUsingLong = ModuleWeaver.LogMethodUsingLong;
        var logMethodUsingTimeSpan = ModuleWeaver.LogMethodUsingTimeSpan;

        if (logWithMessageMethodUsingLong is null && logWithMessageMethodUsingTimeSpan is null)
        {
            if (logMethodUsingLong is null && logMethodUsingTimeSpan is null)
            {
                yield return Instruction.Create(OpCodes.Ldstr, Method.MethodName());
                yield return Instruction.Create(OpCodes.Ldarg_0);
                yield return Instruction.Create(OpCodes.Ldfld, stopwatchFieldReference);
                yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds);
                yield return Instruction.Create(OpCodes.Box, ModuleWeaver.TypeSystem.Int64Reference);
                yield return Instruction.Create(OpCodes.Ldstr, "ms");
                yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ConcatMethod);
                yield return Instruction.Create(OpCodes.Call, ModuleWeaver.TraceWriteLineMethod);
            }
            else
            {
                yield return Instruction.Create(OpCodes.Ldtoken, Method);
                yield return Instruction.Create(OpCodes.Ldtoken, Method.DeclaringType);
                yield return Instruction.Create(OpCodes.Call, ModuleWeaver.GetMethodFromHandle);
                yield return Instruction.Create(OpCodes.Ldarg_0);
                yield return Instruction.Create(OpCodes.Ldfld, stopwatchFieldReference);

                if (logMethodUsingTimeSpan is null)
                {
                    yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds);
                    yield return Instruction.Create(OpCodes.Call, logMethodUsingLong);
                }
                else
                {
                    yield return Instruction.Create(OpCodes.Call, ModuleWeaver.Elapsed);
                    yield return Instruction.Create(OpCodes.Call, logMethodUsingTimeSpan);
                }
            }
        }
        else
        {
            // Important notes:
            // 1. Because async works with state machines, use the state machine & fields instead of method & variables.
            // 2. The ldarg_0 calls are required to load the state machine class and is required before every field call.

            var formattedFieldDefinition = stateMachineTypeDefinition.Fields.FirstOrDefault(x => x.Name.Equals("methodTimerMessage"));
            if (formattedFieldDefinition is null)
            {
                formattedFieldDefinition = new FieldDefinition("methodTimerMessage", FieldAttributes.Private | FieldAttributes.CompilerControlled, ModuleWeaver.TypeSystem.StringReference);
                stateMachineTypeDefinition.Fields.Add(formattedFieldDefinition);
            }

            var formattedFieldReference = new FieldReference(formattedFieldDefinition.Name, formattedFieldDefinition.FieldType, stateMachineTypeReference);

            foreach (var instruction in ProcessTimeAttribute(Method, formattedFieldReference))
            {
                yield return instruction;
            }

            // Handle call to log method
            yield return Instruction.Create(OpCodes.Ldtoken, Method);
            yield return Instruction.Create(OpCodes.Ldtoken, Method.DeclaringType);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.GetMethodFromHandle);
            yield return Instruction.Create(OpCodes.Ldarg_0);
            yield return Instruction.Create(OpCodes.Ldfld, stopwatchFieldReference);

            if (logWithMessageMethodUsingTimeSpan is null)
            {
                yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds);
                yield return Instruction.Create(OpCodes.Ldarg_0);
                yield return Instruction.Create(OpCodes.Ldfld, formattedFieldDefinition);
                yield return Instruction.Create(OpCodes.Call, logWithMessageMethodUsingLong);
            }
            else
            {
                yield return Instruction.Create(OpCodes.Call, ModuleWeaver.Elapsed);
                yield return Instruction.Create(OpCodes.Ldarg_0);
                yield return Instruction.Create(OpCodes.Ldfld, formattedFieldDefinition);
                yield return Instruction.Create(OpCodes.Call, logWithMessageMethodUsingTimeSpan);
            }
        }

        yield return Instruction.Create(OpCodes.Ret);
    }

    IEnumerable<Instruction> ProcessTimeAttribute(MethodDefinition methodDefinition, FieldReference formattedFieldReference)
    {
        // Load everything for a string format
        var timeAttribute = methodDefinition.GetTimeAttribute();
        if (timeAttribute != null)
        {
            var value = timeAttribute.ConstructorArguments.FirstOrDefault().Value as string;
            if (!string.IsNullOrWhiteSpace(value))
            {
                // Note: no need to validate, already done in AssemblyProcessor::ProcessMethod
                var info = parameterFormattingProcessor.ParseParameterFormatting(value);

                yield return Instruction.Create(OpCodes.Ldarg_0);
                yield return Instruction.Create(OpCodes.Ldstr, info.Format);
                yield return Instruction.Create(OpCodes.Ldc_I4, info.ParameterNames.Count);
                yield return Instruction.Create(OpCodes.Newarr, ModuleWeaver.TypeSystem.ObjectReference);

                for (var i = 0; i < info.ParameterNames.Count; i++)
                {
                    var parameterName = info.ParameterNames[i];

                    if (string.Equals(parameterName, "this"))
                    {
                        // Field name is <>4__this
                        parameterName = "<>4__this";

                        // {this} could be optimized away, let's add it for the user
                        InjectThisIntoStateMachine(methodDefinition);
                    }

                    yield return Instruction.Create(OpCodes.Dup);
                    yield return Instruction.Create(OpCodes.Ldc_I4, i);

                    var field = stateMachineTypeDefinition.Fields.FirstOrDefault(x => x.Name.Equals(parameterName));
                    if (field is null)
                    {
                        ModuleWeaver.LogError($"Parameter '{parameterName}' is not available on the async state machine. Probably it has been optimized away by the compiler. Please update the format so it excludes this parameter.");

                        // To make sure the weaver still produces valid IL, pass in a null value
                        yield return Instruction.Create(OpCodes.Ldnull);
                    }
                    else
                    {
                        yield return Instruction.Create(OpCodes.Ldarg_0);
                        yield return Instruction.Create(OpCodes.Ldfld, field);

                        if (field.FieldType.IsBoxingRequired(ModuleWeaver.TypeSystem.ObjectReference))
                        {
                            yield return Instruction.Create(OpCodes.Box, ModuleWeaver.ModuleDefinition.ImportReference(field.FieldType));
                        }
                    }

                    yield return Instruction.Create(OpCodes.Stelem_Ref);
                }

                yield return Instruction.Create(OpCodes.Call, ModuleWeaver.StringFormatWithArray);
            }
            else
            {
                // Load null a string
                yield return Instruction.Create(OpCodes.Ldarg_0);
                yield return Instruction.Create(OpCodes.Ldnull);
            }

            yield return Instruction.Create(OpCodes.Stfld, formattedFieldReference);
        }
    }

    void InjectThisIntoStateMachine(MethodDefinition methodDefinition)
    {
        const string fieldName = "<>4__this";

        if (stateMachineTypeDefinition.Fields.Any(x => x.Name.Equals(fieldName)))
        {
            return;
        }

        // Step 1: inject the field
        var thisField = new FieldDefinition(fieldName, FieldAttributes.Public, methodDefinition.DeclaringType);
        stateMachineTypeDefinition.Fields.Add(thisField);

        // Step 2: set the field value in the actual method, search for the first usage of the state machine class
        var methodBody = methodDefinition.Body;
        methodBody.SimplifyMacros();

        var instructions = methodDefinition.Body.Instructions;
        var startInstruction = instructions.FirstOrDefault(x =>
        {
            if (x.Operand is VariableDefinition variableDefinition)
            {
                if (variableDefinition.VariableType.FullName.Equals(stateMachineTypeDefinition.FullName))
                {
                    return true;
                }
            }

            return false;
        });

        if (startInstruction is null)
        {
            ModuleWeaver.LogError("Failed to inject '{this}' into the async method, the compiler optimized it away and it could not be injected automatically, please create a support ticket.");
            return;
        }

        // IL_0000: ldloca.s     'stateMachine [Range(Instruction(IL_0000 ldloca.s)-Instruction(IL_003c ldloca.s))]'
        // IL_0002: ldarg.0      // this
        // IL_0003: stfld        class ClassWithAsyncMethod ClassWithAsyncMethod/'<MethodWithAwaitAndThisAsync>d__1'::'<>4__this'

        var index = instructions.IndexOf(startInstruction);
        instructions.Insert(index + 0, Instruction.Create(OpCodes.Ldloca_S, (VariableDefinition)startInstruction.Operand));
        instructions.Insert(index + 1, Instruction.Create(OpCodes.Ldarg_0));
        instructions.Insert(index + 2, Instruction.Create(OpCodes.Stfld, thisField));

        methodBody.OptimizeMacros();
    }
}