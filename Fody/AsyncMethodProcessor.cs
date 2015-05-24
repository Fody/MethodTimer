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
            throw new WeavingException(string.Format("An error occurred processing '{0}'. Error: {1}", Method.FullName, exception.Message));
        }
    }

    void InnerProcess()
    {
        var asyncAttribute = Method.GetAsyncStateMachineAttribute();
        stateMachineType = asyncAttribute.ConstructorArguments
            .Select(ctor => (TypeDefinition) ctor.Value)
            .Single();
        var moveNextMethod = stateMachineType.Methods
            .Single(x => x.Name == "MoveNext");
        body = moveNextMethod.Body;

        var first = body.Instructions.FirstOrDefault(x => x.SequencePoint != null && x.SequencePoint.StartLine != 0xfeefee);

        returnPoints = GetAsyncReturns(body.Instructions)
            .ToList();
        if (first == null)
        {
            var message = string.Format("Cannot find start point of async method '{0}.{1}', method will be skipped. Ensure you are producing pdb as part of your build", Method.DeclaringType.Name, Method.Name);
            ModuleWeaver.LogWarning(message);
            return;
        }
        if (first.OpCode == OpCodes.Nop)
        {
            first = first.Next;
        }
        body.SimplifyMacros();
        InjectStopwatch(body.Instructions.IndexOf(first.Next));
        HandleReturns();
        body.InitLocals = true;
        body.OptimizeMacros();
    }

    void InjectStopwatch(int index)
    {
        stopwatchField = new FieldDefinition("methodTimerStopwatch", new FieldAttributes(), ModuleWeaver.StopwatchType);
        stateMachineType.Fields.Add(stopwatchField);
        body.Insert(index, new[]
        {
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

        var possibleReturnStatements = new List<Instruction>();

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
                    // This is a return statement
                    yield return instruction;
                }
            }
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
        foreach (var instruction in GetWriteTimeInstruction())
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


    IEnumerable<Instruction> GetWriteTimeInstruction()
    {
        yield return Instruction.Create(OpCodes.Ldarg_0);
        yield return Instruction.Create(OpCodes.Ldfld, stopwatchField);
        yield return Instruction.Create(OpCodes.Call, ModuleWeaver.StopMethod);
        if (ModuleWeaver.LogMethod == null)
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
        else
        {
            yield return Instruction.Create(OpCodes.Ldtoken, Method);
            yield return Instruction.Create(OpCodes.Ldtoken, Method.DeclaringType);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.GetMethodFromHandle);
            yield return Instruction.Create(OpCodes.Ldarg_0);
            yield return Instruction.Create(OpCodes.Ldfld, stopwatchField);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.LogMethod);
        }
    }
}

