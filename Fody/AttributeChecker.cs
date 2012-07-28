using System.Linq;
using Mono.Cecil;

public static class AttributeChecker
{
    public static bool ContainsTimeAttribute(this ICustomAttributeProvider definition)
    {
        return definition.CustomAttributes
            .Any(x => x.AttributeType.Name == "TimeAttribute");
    }
}