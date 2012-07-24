using System;
using System.Linq;
using Mono.Cecil;


public class MethodProcessor
{
    TypeSystem typeSystem;

    public MethodProcessor(ModuleDefinition moduleDefinition)
    {
        typeSystem = moduleDefinition.TypeSystem;
    }

    public void ProcessAttributes(IMemberDefinition memberDefinition)
    {
        try
        {
            InnerProcess(memberDefinition);
        }
        catch (Exception exception)
        {
            throw new WeavingException(string.Format("An error occurred processing '{0}'. Error: {1}", memberDefinition.FullName, exception.Message));
        }
    }

    void InnerProcess(IMemberDefinition memberDefinition)
    {
        var customAttributes = memberDefinition.CustomAttributes;
        var attribute = customAttributes.FirstOrDefault(x => x.AttributeType.Name == "TimeAttribute");
        if (attribute == null)
        {
            return;
        }

        customAttributes.Remove(attribute);

    }

}