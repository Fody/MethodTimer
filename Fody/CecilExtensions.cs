using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

public static class CecilExtensions
{
    public static IEnumerable<MethodDefinition> AbstractMethods(this TypeDefinition type)
    {
        return type.Methods.Where(x=>x.IsAbstract);
    }
    public static IEnumerable<MethodDefinition> ConcreteMethods(this TypeDefinition type)
    {
        return type.Methods.Where(x=>!x.IsAbstract  && !IsEmptyConstructor(x));
    }
     static bool IsEmptyConstructor(this MethodDefinition method)
    {
        return ((method.Name == ".ctor") && (method.HasBody) && (method.Body.Instructions.Count == 3));
    }

    public static string MethodName(this MethodDefinition method)
    {
        if (method.IsConstructor)
        {
            return string.Format("{0}{1} ", method.DeclaringType.Name, method.Name);
        }
        return string.Format("{0}.{1} ", method.DeclaringType.Name, method.Name);
    }
    public static void Insert(this Collection<Instruction> collection, int index,  List<Instruction> instructions)
    {
        instructions.Reverse();
        foreach (var instruction in instructions)
        {
            collection.Insert(index, instruction);
        }   
    }
    public static void Add(this Collection<Instruction> collection,  params Instruction[] instructions)
    {
        foreach (var instruction in instructions)
        {
            collection.Add(instruction);
        }   
    }
}