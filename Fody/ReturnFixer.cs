using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

public class ReturnFixer
{
    public MethodDefinition Method;
    public Instruction NopForHandleEnd;
    Collection<Instruction> instructions;
    public Instruction NopBeforeReturn;
    Instruction sealBranchesNop;


    public void  MakeLastStatementReturn()
    {

          instructions = Method.Body.Instructions;
        FixHangingHandlerEnd();

        sealBranchesNop = Instruction.Create(OpCodes.Nop);
       instructions.Add(sealBranchesNop);

        NopBeforeReturn = Instruction.Create(OpCodes.Nop);

        foreach (var instruction in instructions)
        {
            var operand = instruction.Operand as Instruction;
            if (operand != null)
            {
                if (operand.OpCode == OpCodes.Ret)
                {
                    instruction.Operand = sealBranchesNop;
                }
            }
        }

        if (Method.MethodReturnType.ReturnType.Name == "Void")
        {
            WithNoReturn();
            return;
        }
        WithReturnValue();
    }

    void FixHangingHandlerEnd()
    {
        NopForHandleEnd = Instruction.Create(OpCodes.Nop);
        Method.Body.Instructions.Add(NopForHandleEnd);
        foreach (var handler in Method.Body.ExceptionHandlers)
        {
            if (handler.HandlerStart != null && handler.HandlerEnd == null)
            {
                handler.HandlerEnd = NopForHandleEnd;
            }
        }
    }


   void WithReturnValue()
    {

        var returnVariable = new VariableDefinition(Method.MethodReturnType.ReturnType);
        Method.Body.Variables.Add(returnVariable);

        for (var index = 0; index < instructions.Count; index++)
        {
            var instruction = instructions[index];
            if (instruction.OpCode == OpCodes.Ret)
            {
                instructions.Insert(index, Instruction.Create(OpCodes.Stloc, returnVariable));
                instruction.OpCode = OpCodes.Br;
                instruction.Operand = sealBranchesNop;
                index++;
            }
        }
        instructions.Add(NopBeforeReturn);
        instructions.Add( Instruction.Create(OpCodes.Ldloc, returnVariable));
        instructions.Add(Instruction.Create(OpCodes.Ret));
        
    }

    void WithNoReturn()
    {

        foreach (var instruction in instructions)
        {
            if (instruction.OpCode == OpCodes.Ret)
            {
                instruction.OpCode = OpCodes.Br;
                instruction.Operand = sealBranchesNop;
            }
        }
        instructions.Add(NopBeforeReturn);
        instructions.Add(Instruction.Create(OpCodes.Ret));
    }

}