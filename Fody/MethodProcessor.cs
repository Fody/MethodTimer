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
    public TypeSystem TypeSystem;
    public MethodDefinition Method;
    MethodBody body;
    VariableDefinition stopwatchVar;
    FieldDefinition stopwatchField;
    TypeDefinition asyncStateMachineType;

    public void Process()
    {
        try
        {
            if (Method.IsAsync())
            {
                InnerProcessAsync();
            }
            else
            {
                InnerProcess();
            }
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

        InjectStopwatch();
        HandleReturns();

        body.InitLocals = true;
        body.OptimizeMacros();
    }

    void InnerProcessAsync()
    {
        var asyncAttribute = Method.GetAsyncStateMachineAttribute();
        asyncStateMachineType = (from ctor in asyncAttribute.ConstructorArguments
                                  select (TypeDefinition)ctor.Value).First();

        var moveNextMethod = (from method in asyncStateMachineType.Methods
                              where string.Equals(method.Name, "MoveNext")
                              select method).First();

        body = moveNextMethod.Body;
        body.SimplifyMacros();

        var startInstructionIndex = FindMethodStartAsync(body.Instructions);
        if (startInstructionIndex != -1)
        {
            InjectStopwatchAsync(asyncStateMachineType, body.Instructions, startInstructionIndex);
            HandleReturnsAsync();
        }
        else
        {
            ModuleWeaver.LogWarning(string.Format("Cannot find start point of async method '{0}.{1}', method will be skipped", 
                Method.DeclaringType.Name, Method.Name));
        }

        body.InitLocals = true;
        body.OptimizeMacros();
    }

    void HandleReturns()
    {
        var instructions = body.Instructions;

        var returnPoints = instructions.Where(x => x.OpCode == OpCodes.Ret).ToList();

        foreach (var returnPoint in returnPoints)
        {
            FixReturn(instructions, returnPoint);
        }

        var last = instructions.Last();
        if (last.OpCode == OpCodes.Rethrow || last.OpCode == OpCodes.Throw)
        {
            FixReturn(instructions, last);
        }
    }

    void HandleReturnsAsync()
    {
        var instructions = body.Instructions;

        // There are 3 possible return points:
        // 
        // 1) async code:
        //      awaiter.GetResult();
        //      awaiter = new TaskAwaiter();
        //
        // 2) exception handling
        //      L_00d5: ldloc.1 
        //      L_00d6: call instance void [mscorlib]System.Runtime.CompilerServices.AsyncTaskMethodBuilder::SetException(class [mscorlib]System.Exception)
        //
        // 3) all other returns
        //
        // We can do this smart by searching for all leave and leave_S op codes and check if they point to the last
        // instruction of the method. This equals a "return" call.

        var possibleReturnStatements = new List<Instruction>();

        for (var i = instructions.Count - 1; i >= 0; i--)
        {
            if (instructions[i].IsLeaveInstruction())
            {
                possibleReturnStatements.Add(instructions[i + 1]);
                break;
            }
        }

        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            if (instruction.IsLeaveInstruction())
            {
                if (possibleReturnStatements.Any(x => ReferenceEquals(instruction.Operand, x)))
                {
                    // This is a return statement
                    var instructionsAdded = FixReturn(instructions, instruction);
                    i += instructionsAdded;
                }
            }
        }
    }

    int FixReturn(Collection<Instruction> instructions, Instruction returnPoint)
    {
        var opCode = returnPoint.OpCode;
        var operand = returnPoint.Operand as Instruction;

        returnPoint.OpCode = OpCodes.Nop;
        returnPoint.Operand = null;

        var instructionsAdded = 0;
        var indexOf = instructions.IndexOf(returnPoint);
        foreach (var instruction in GetWriteTimeInstructions())
        {
            indexOf++;
            instructions.Insert(indexOf, instruction);
            instructionsAdded++;
        }

        indexOf++;

        if ((opCode == OpCodes.Leave) || (opCode == OpCodes.Leave_S))
        {
            instructions.Insert(indexOf, Instruction.Create(opCode, operand));
            instructionsAdded++;
        }
        else
        {
            instructions.Insert(indexOf, Instruction.Create(opCode));
            instructionsAdded++;
        }

        return instructionsAdded;
    }

    IEnumerable<Instruction> GetWriteTimeInstructions()
    {
        foreach (var instruction in GetLoadStopwatchInstruction())
        {
            yield return instruction;
        }

        yield return Instruction.Create(OpCodes.Call, ModuleWeaver.StopMethod);
        if (ModuleWeaver.LogMethod == null)
        {
            yield return Instruction.Create(OpCodes.Ldstr, Method.MethodName());

            foreach (var instruction in GetLoadStopwatchInstruction())
            {
                yield return instruction;
            }

            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Box, TypeSystem.Int64);
            yield return Instruction.Create(OpCodes.Ldstr, "ms");
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ConcatMethod);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.DebugWriteLineMethod);
        }
        else
        {
            yield return Instruction.Create(OpCodes.Ldtoken, Method);
            yield return Instruction.Create(OpCodes.Ldtoken, Method.DeclaringType);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.GetMethodFromHandle);

            foreach (var instruction in GetLoadStopwatchInstruction())
            {
                yield return instruction;
            }

            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.LogMethod);
        }
    }

    Instruction[] GetLoadStopwatchInstruction()
    {
        if (stopwatchVar != null)
        {
            return new[]
            {
                Instruction.Create(OpCodes.Ldloc, stopwatchVar)
            };
        }

        if (stopwatchField != null)
        {
            return new[]
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, stopwatchField)
            };
        }

        return new Instruction[] { };
    }

    void InjectStopwatch()
    {
        // inject as variable
        stopwatchVar = new VariableDefinition("methodTimerStopwatch", ModuleWeaver.StopwatchType);
        body.Variables.Add(stopwatchVar);

        body.Instructions.Insert(0, new List<Instruction>(new[] {
            Instruction.Create(OpCodes.Call, ModuleWeaver.StartNewMethod),
            Instruction.Create(OpCodes.Stloc, stopwatchVar)
        }));
    }

    void InjectStopwatchAsync(TypeDefinition typeDefinition, Collection<Instruction> instructions, int instructionIndex)
    {
        // inject as field
        stopwatchField = new FieldDefinition("methodTimerStopwatch", new FieldAttributes(), ModuleWeaver.StopwatchType);
        typeDefinition.Fields.Add(stopwatchField);

        // This code:
        // 
        // if (_stopwatchField == null)
        // {
        //     _stopwatchField = Stopwatch.StartNew();
        // }
        //
        // Translated to IL:
        //
        // L_0025: ldarg.0 
        // L_0026: ldfld class [System]System.Diagnostics.Stopwatch ClassWithAsyncMethod/<MethodWithAwaitExpected>d__3::<stopwatch>5__4
        // L_002b: ldnull 
        // L_002c: ceq 
        // L_002e: ldc.i4.0 
        // L_002f: ceq 
        // L_0031: stloc.3 <== bool variable
        // L_0032: ldloc.3 <== bool variable
        // L_0033: brtrue.s L_0042 <== first instruction after stfld
        // L_0035: nop 
        // L_0036: ldarg.0 
        // L_0037: call class [System]System.Diagnostics.Stopwatch [System]System.Diagnostics.Stopwatch::StartNew()
        // L_003c: stfld class [System]System.Diagnostics.Stopwatch ClassWithAsyncMethod/<MethodWithAwaitExpected>d__3::<stopwatch>5__4


        var isNullBoolVariable = new VariableDefinition("isStopwatchNull", ModuleWeaver.BooleanType);
        body.Variables.Add(isNullBoolVariable);

        var startInstruction = instructions[instructionIndex];

        instructions.Insert(instructionIndex, new List<Instruction>(new[] {
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, stopwatchField),
            Instruction.Create(OpCodes.Ldnull),
            Instruction.Create(OpCodes.Ceq),
            Instruction.Create(OpCodes.Ldc_I4, 0),
            Instruction.Create(OpCodes.Ceq),
            Instruction.Create(OpCodes.Stloc, isNullBoolVariable),
            Instruction.Create(OpCodes.Ldloc, isNullBoolVariable),
            Instruction.Create(OpCodes.Brtrue_S, startInstruction),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, ModuleWeaver.StartNewMethod),
            Instruction.Create(OpCodes.Stfld, stopwatchField)
        }));
    }

    int FindMethodStartAsync(Collection<Instruction> instructions)
    {
        var startIndex = -1;
        Instruction startInstruction = null;

        // V1: Inject stopwatch to beginning of "default" label
        //startInstruction = FindMethodStartInstructionAsyncV1(instructions);

        // V2: Inject at the beginning, check if item is null
        startInstruction = FindMethodStartInstructionAsyncV2(instructions);

        // If instruction is nop, increase index
        if (startInstruction != null)
        {
            startIndex = instructions.IndexOf(startInstruction);
            if (startInstruction.OpCode == OpCodes.Nop)
            {
                startIndex++;
            }
        }

        return startIndex;
    }

    Instruction FindMethodStartInstructionAsyncV2(Collection<Instruction> instructions)
    {
        return instructions.First();
    }

}