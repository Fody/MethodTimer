using System.Collections.Generic;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

public static class CecilExstensions
{
    public static void BeforeLast(this Collection<Instruction> collection, Instruction instruction)
    {
        var index = collection.Count - 1;
        collection.Insert(index, instruction);
    }

    public static void Append(this Collection<Instruction> collection, List<Instruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            collection.Add(instruction);
        }
    }
}