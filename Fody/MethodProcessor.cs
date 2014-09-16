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
    MethodBody _body;
    VariableDefinition _stopwatchVar;
    FieldDefinition _stopwatchField;
    TypeDefinition _asyncStateMachineType;

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
        _body = Method.Body;
        _body.SimplifyMacros();

        InjectStopwatch();
        HandleReturns();

        _body.InitLocals = true;
        _body.OptimizeMacros();
    }

    void InnerProcessAsync()
    {
        // Find state machine type
        var asyncAttribute = Method.GetAsyncStateMachineAttribute();
        _asyncStateMachineType = (from ctor in asyncAttribute.ConstructorArguments
                                  select (TypeDefinition)ctor.Value).First();

        // Find the MoveNext method
        var moveNextMethod = (from method in _asyncStateMachineType.Methods
                              where string.Equals(method.Name, "MoveNext")
                              select method).First();

        _body = moveNextMethod.Body;
        _body.SimplifyMacros();

        // Find the real start of the "method"
        var startInstructionIndex = FindMethodStartAsync(_body.Instructions);

        // Inject the stopwatch
        InjectStopwatchAsync(_asyncStateMachineType, _body.Instructions, startInstructionIndex);

        // Handle the returns in async mode
        HandleReturnsAsync();

        _body.InitLocals = true;
        _body.OptimizeMacros();
    }

    void HandleReturns()
    {
        var instructions = _body.Instructions;

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
        var instructions = _body.Instructions;

        // There are 2 possible return points:

        // 1) async code:
        //      awaiter.GetResult();
        //      awaiter = new TaskAwaiter();

        var getResultInstruction = (from instruction in instructions
                                    where instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference
                                          && string.Equals(((MethodReference)instruction.Operand).Name, "GetResult")
                                    select instruction).First();

        var getResultIndex = instructions.IndexOf(getResultInstruction);

        var nextLeaveStatement = 0;
        for (var i = getResultIndex; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            if ((instruction.OpCode == OpCodes.Leave) || (instruction.OpCode == OpCodes.Leave_S))
            {
                nextLeaveStatement = i;
                break;
            }
        }

        if (instructions[nextLeaveStatement - 1].OpCode == OpCodes.Nop)
        {
            nextLeaveStatement--;
        }

        var finalInstruction = instructions[nextLeaveStatement];

        FixReturn(instructions, finalInstruction);

        // 2) exception handling
        //      L_00d5: ldloc.1 
        //      L_00d6: call instance void [mscorlib]System.Runtime.CompilerServices.AsyncTaskMethodBuilder::SetException(class [mscorlib]System.Exception)

        var setExceptionMethod = (from instruction in instructions
                                  where instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference
                                        && string.Equals(((MethodReference)instruction.Operand).Name, "SetException")
                                  select instruction).First();

        var setExceptionMethodIndex = instructions.IndexOf(setExceptionMethod);

        FixReturn(instructions, instructions[setExceptionMethodIndex + 1]);
    }

    void FixReturn(Collection<Instruction> instructions, Instruction returnPoint)
    {
        var opCode = returnPoint.OpCode;
        var operand = returnPoint.Operand as Instruction;

        returnPoint.OpCode = OpCodes.Nop;
        returnPoint.Operand = null;

        var indexOf = instructions.IndexOf(returnPoint);
        foreach (var instruction in GetWriteTimeIL())
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

    private IEnumerable<Instruction> GetWriteTimeIL()
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
        if (_stopwatchVar != null)
        {
            return new[]
            {
                Instruction.Create(OpCodes.Ldloc, _stopwatchVar)
            };
        }

        if (_stopwatchField != null)
        {
            return new[]
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, _stopwatchField)
            };
        }

        return new Instruction[] { };
    }

    void InjectStopwatch()
    {
        // inject as variable
        _stopwatchVar = new VariableDefinition("methodTimerStopwatch", ModuleWeaver.StopwatchType);
        _body.Variables.Add(_stopwatchVar);

        _body.Instructions.Insert(0, new List<Instruction>(new[] {
            Instruction.Create(OpCodes.Call, ModuleWeaver.StartNewMethod),
            Instruction.Create(OpCodes.Stloc, _stopwatchVar)
        }));
    }

    void InjectStopwatchAsync(TypeDefinition typeDefinition, Collection<Instruction> instructions, int instructionIndex)
    {
        // inject as field
        _stopwatchField = new FieldDefinition("methodTimerStopwatch", new FieldAttributes(), ModuleWeaver.StopwatchType);
        typeDefinition.Fields.Add(_stopwatchField);

        instructions.Insert(instructionIndex, new List<Instruction>(new[] {
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, ModuleWeaver.StartNewMethod),
            Instruction.Create(OpCodes.Stfld, _stopwatchField)
        }));
    }

    static int FindMethodStartAsync(Collection<Instruction> instructions)
    {
        // Inject stopwatch to beginning of "default" label, which is the first address after the ldc.i4.0:
        // L_000f: ldc.i4.0         <== ldc.i4.0 which we are looking for
        // L_0010: beq.s L_0016     
        // L_0012: br.s L_0018      <== first br.s that jumpts to address #1
        // L_0014: br.s L_0083
        // L_0016: br.s L_0055      <== br.s that jumps to the actual start of the "method"
        // L_0018: br.s L_001a

        var ldcLoadIndex = (from instruction in instructions
                            where (instruction.OpCode == OpCodes.Ldc_I4_0) ||
                                  (instruction.OpCode == OpCodes.Ldc_I4 && (int)instruction.Operand == 0)
                            select instructions.IndexOf(instruction)).First();

        var address1JumpIndex = ldcLoadIndex + 2;

        var address2JumpInstruction = (Instruction)instructions[address1JumpIndex].Operand;

        var startInstruction = (Instruction)address2JumpInstruction.Operand;
        var startInstructionIndex = instructions.IndexOf(startInstruction);

        if (startInstruction.OpCode == OpCodes.Nop)
        {
            startInstructionIndex++;
        }

        return startInstructionIndex;
    }
}