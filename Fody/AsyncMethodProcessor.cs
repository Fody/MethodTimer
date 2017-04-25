using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

public class AsyncMethodProcessor
{
    public ModuleWeaver ModuleWeaver;
    public MethodDefinition Method;
    MethodBody body;
    FieldDefinition stopwatchField;
    TypeDefinition stateMachineType;
    List<Instruction> returnPoints;

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
        stateMachineType = asyncAttribute.ConstructorArguments
            .Select(ctor => (TypeDefinition)ctor.Value)
            .Single();
        var moveNextMethod = stateMachineType.Methods
            .Single(x => x.Name == "MoveNext");
        body = moveNextMethod.Body;

        body.SimplifyMacros();

        returnPoints = GetAsyncReturns(body.Instructions)
            .ToList();

        // First, fall back to old mechanism
        var exceptionHandler = body.ExceptionHandlers.First();
        var index = body.Instructions.IndexOf(exceptionHandler.TryStart);

        // Check roslyn usage
        var firstStateUsage = (from instruction in body.Instructions
                               let fieldReference = instruction.Operand as FieldReference
                               where instruction.OpCode == OpCodes.Ldfld &&
                                     fieldReference != null &&
                                     fieldReference.Name.Contains("__state")
                               select instruction).FirstOrDefault();
        if (firstStateUsage == null)
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

        InjectStopwatch(index, body.Instructions[index]);

        HandleReturns();
        body.InitLocals = true;
        body.OptimizeMacros();
    }

    void InjectStopwatch(int index, Instruction nextInstruction)
    {
        var boolVariable = new VariableDefinition(ModuleWeaver.BooleanType.Resolve());
        body.Variables.Add(boolVariable);

        stopwatchField = new FieldDefinition("methodTimerStopwatch", new FieldAttributes(), ModuleWeaver.StopwatchType);
        stateMachineType.Fields.Add(stopwatchField);
        body.Insert(index, new[]
        {
            // This code looks like this:
            // if (_stopwatch == null)
            // {
            //    _stopwatch = Stopwatch.StartNew();
            // }

            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, stopwatchField),
            Instruction.Create(OpCodes.Ldnull),
            Instruction.Create(OpCodes.Ceq),
            Instruction.Create(OpCodes.Stloc, boolVariable),
            Instruction.Create(OpCodes.Ldloc, boolVariable),
            Instruction.Create(OpCodes.Brfalse_S, nextInstruction),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, ModuleWeaver.StartNewMethod),
            Instruction.Create(OpCodes.Stfld, stopwatchField)
        });
    }

    void HandleReturns()
    {
        foreach (var returnPoint in returnPoints)
        {
            FixReturn(returnPoint);
        }
    }

    static IEnumerable<Instruction> GetAsyncReturns(Collection<Instruction> instructions)
    {
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

        var returnStatements = new List<Instruction>();

        var possibleReturnStatements = new List<Instruction>();

        // Look for the last leave statement (that is the line all "return" statements go to)
        for (var i = instructions.Count - 1; i >= 0; i--)
        {
            var instruction = instructions[i];
            if (instruction.IsLeaveInstruction())
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
                    // This is a return statement, this covers scenarios 1 and 3
                    returnStatements.Add(instruction);
                }
                else
                {
                    // Check if we set an exception in this block, this covers scenario 2
                    for (var j = i - 3; j < i; j++)
                    {
                        var previousInstruction = instructions[j];
                        if (previousInstruction.OpCode == OpCodes.Call)
                        {
                            var methodReference = previousInstruction.Operand as MethodReference;
                            if (methodReference != null)
                            {
                                if (methodReference.Name.Equals("SetException"))
                                {
                                    returnStatements.Add(instruction);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        return returnStatements;
    }

    void FixReturn(Instruction returnPoint)
    {
        var opCode = returnPoint.OpCode;
        var operand = returnPoint.Operand as Instruction;

        returnPoint.OpCode = OpCodes.Nop;
        returnPoint.Operand = null;

        var instructions = body.Instructions;
        var indexOf = instructions.IndexOf(returnPoint);
        foreach (var instruction in GetWriteTimeInstruction())
        {
            indexOf++;
            instructions.Insert(indexOf, instruction);
        }

        indexOf++;

        if (opCode == OpCodes.Leave || opCode == OpCodes.Leave_S)
        {
            instructions.Insert(indexOf, Instruction.Create(opCode, operand));
        }
        else
        {
            instructions.Insert(indexOf, Instruction.Create(opCode));
        }
    }


    IEnumerable<Instruction> GetWriteTimeInstruction()
    {
        yield return Instruction.Create(OpCodes.Ldarg_0);
        yield return Instruction.Create(OpCodes.Ldfld, stopwatchField);
        yield return Instruction.Create(OpCodes.Call, ModuleWeaver.StopMethod);

        var logWithMessageMethod = ModuleWeaver.LogWithMessageMethod;
        var logMethod = ModuleWeaver.LogMethod;

        if (logWithMessageMethod != null)
        {
            // TODO: Implement
        }
        else if (logMethod != null)
        {
            yield return Instruction.Create(OpCodes.Ldtoken, Method);
            yield return Instruction.Create(OpCodes.Ldtoken, Method.DeclaringType);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.GetMethodFromHandle);
            yield return Instruction.Create(OpCodes.Ldarg_0);
            yield return Instruction.Create(OpCodes.Ldfld, stopwatchField);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Call, logMethod);
        }
        else
        {
            yield return Instruction.Create(OpCodes.Ldstr, Method.MethodName());
            yield return Instruction.Create(OpCodes.Ldarg_0);
            yield return Instruction.Create(OpCodes.Ldfld, stopwatchField);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Box, ModuleWeaver.ModuleDefinition.TypeSystem.Int64);
            yield return Instruction.Create(OpCodes.Ldstr, "ms");
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ConcatMethod);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.DebugWriteLineMethod);
        }
    }
}

