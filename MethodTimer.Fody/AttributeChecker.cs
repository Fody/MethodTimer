using System.Linq;
using Mono.Cecil;

public static class AttributeChecker
{
    public static CustomAttribute GetTimeAttribute(this ICustomAttributeProvider definition)
    {
        var customAttributes = definition.CustomAttributes;

        return customAttributes.FirstOrDefault(_ => _.AttributeType.Name == "TimeAttribute");
    }

    public static bool ContainsTimeAttribute(this ICustomAttributeProvider definition) =>
        GetTimeAttribute(definition) != null;

    public static bool IsCompilerGenerated(this ICustomAttributeProvider definition)
    {
        var customAttributes = definition.CustomAttributes;

        return customAttributes.Any(_ => _.AttributeType.Name == "CompilerGeneratedAttribute");
    }

    public static void RemoveTimeAttribute(this ICustomAttributeProvider definition)
    {
        var customAttributes = definition.CustomAttributes;

        var timeAttribute = customAttributes.FirstOrDefault(_ => _.AttributeType.Name == "TimeAttribute");

        if (timeAttribute != null)
        {
            customAttributes.Remove(timeAttribute);
        }
    }
}