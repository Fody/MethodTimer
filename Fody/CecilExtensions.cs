using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

public static class CecilExtensions
{
    public static bool IsBoxingRequired(this TypeReference typeReference, TypeReference expectedType)
    {
        if (expectedType.IsValueType && string.Equals(typeReference.FullName, expectedType.FullName))
        {
            // Boxing is never required if type is expected
            return false;
        }

        if (typeReference.IsValueType || typeReference.IsGenericParameter)
        {
            return true;
        }

        return false;
    }

    public static IEnumerable<MethodDefinition> AbstractMethods(this TypeDefinition type)
    {
        return type.Methods.Where(x => x.IsAbstract);
    }

    public static IEnumerable<MethodDefinition> ConcreteMethods(this TypeDefinition type)
    {
        return type.Methods.Where(x => !x.IsAbstract && x.HasBody && !IsEmptyConstructor(x));
    }

    static bool IsEmptyConstructor(this MethodDefinition method)
    {
        return method.Name == ".ctor" && method.Body.Instructions.Count(x=>x.OpCode!=OpCodes.Nop) == 3;
    }

    public static bool IsInterceptor(this TypeReference type)
    {
        return type.Name == "MethodTimeLogger";
    }
    public static bool IsInstanceConstructor(this MethodDefinition methodDefinition)
    {
        return methodDefinition.IsConstructor && !methodDefinition.IsStatic;
    }

    public static void InsertBefore(this MethodBody body, Instruction target, Instruction instruction)
    {
        body.Instructions.InsertBefore(target, instruction);
    }

    public static void InsertBefore(this Collection<Instruction> instructions, Instruction target, Instruction instruction)
    {
        var index = instructions.IndexOf(target);
        instructions.Insert(index, instruction);
    }

    public static string MethodName(this MethodDefinition method)
    {
        if (method.IsConstructor)
        {
            return $"{method.DeclaringType.Name}{method.Name} ";
        }
        return $"{method.DeclaringType.Name}.{method.Name} ";
    }

    public static void Insert(this MethodBody body, int index, IEnumerable<Instruction> instructions)
    {
        instructions = instructions.Reverse();
        foreach (var instruction in instructions)
        {
            body.Instructions.Insert(index, instruction);
        }
    }

    public static void Add(this MethodBody body, params Instruction[] instructions)
    {
        foreach (var instruction in instructions)
        {
            body.Instructions.Add(instruction);
        }
    }

    public static bool IsYield(this MethodDefinition method)
    {
        if (method.ReturnType == null)
        {
            return false;
        }
        if (!method.ReturnType.Name.StartsWith("IEnumerable"))
        {
            return false;
        }
        var stateMachinePrefix = $"<{method.Name}>";
        var nestedTypes = method.DeclaringType.NestedTypes;
        return nestedTypes.Any(x => x.Name.StartsWith(stateMachinePrefix));
    }

    public static CustomAttribute GetAsyncStateMachineAttribute(this MethodDefinition method)
    {
        return method.CustomAttributes.FirstOrDefault(_ => _.AttributeType.Name == "AsyncStateMachineAttribute");
    }

    public static bool IsAsync(this MethodDefinition method)
    {
        return GetAsyncStateMachineAttribute(method) != null;
    }

    public static bool IsLeaveInstruction(this Instruction instruction)
    {
        return instruction.OpCode == OpCodes.Leave || instruction.OpCode == OpCodes.Leave_S;
    }

}