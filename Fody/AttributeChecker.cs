using System.Linq;
using Mono.Cecil;

public static class AttributeChecker
{
    public static bool ContainsTimeAttribute(this ICustomAttributeProvider definition)
    {
        var customAttributes = definition.CustomAttributes;

        return customAttributes.Any(x => x.AttributeType.Name == "TimeAttribute");
    }

    public static void RemoveTimeAttribute(this ICustomAttributeProvider definition)
    {
        var customAttributes = definition.CustomAttributes;

        var timeAttribute = customAttributes.FirstOrDefault(x => x.AttributeType.Name == "TimeAttribute");

        if (timeAttribute != null)
        {
            customAttributes.Remove(timeAttribute);
        }

    }
}