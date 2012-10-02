using System.Linq;
using Mono.Cecil;

public static class AttributeChecker
{
    public static bool ContainsTimeAttribute(this ICustomAttributeProvider definition)
    {
        var customAttributes = definition.CustomAttributes;

        var timeAttributes = customAttributes
            .Where(x => x.AttributeType.Name == "TimeAttribute")
            .ToList();
        foreach (var attribute in timeAttributes)
        {
            customAttributes.Remove(attribute);
        }
        return timeAttributes.Any();
    }
}