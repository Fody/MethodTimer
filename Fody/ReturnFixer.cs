using System.Linq;
using Mono.Cecil.Cil;

public static class ReturnFixer
{
    public static Instruction MakeLastStatementReturn(this MethodBody method)
    {
        var instructions = method.Instructions;
        var last = instructions.Last();

        var count = instructions.Count - 1;
        Instruction nopBeforeReturn;

        var beforeLast = instructions[count];

        if (beforeLast.OpCode.ToString().StartsWith("ld"))
        {
            count--;
            beforeLast = instructions[count];
        }
        if (beforeLast.OpCode == OpCodes.Nop)
        {
            nopBeforeReturn = beforeLast;
            count--;
        }
        else
        {
            nopBeforeReturn = Instruction.Create(OpCodes.Nop);
            instructions.BeforeLast(nopBeforeReturn);
        }


        foreach (var exceptionHandler in method.ExceptionHandlers)
        {
            if (exceptionHandler.HandlerEnd == last)
            {
                exceptionHandler.HandlerEnd = nopBeforeReturn;
            }
        }
        for (var index = 0; index < count; index++)
        {
            var instruction = instructions[index];
            if (instruction.OpCode == OpCodes.Ret)
            {
                instructions[index] = Instruction.Create(OpCodes.Br, nopBeforeReturn);
            }
            if (instruction.Operand == last)
            {
                instructions[index] = Instruction.Create(instruction.OpCode, nopBeforeReturn);
            }
        }
        return nopBeforeReturn;
    }
}