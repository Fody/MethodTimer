using System.Collections.Generic;
using Mono.Cecil;

public class AllTypesFinder
{
    ModuleDefinition moduleDefinition;

    public List<TypeDefinition> AllTypes;
    public AllTypesFinder(ModuleDefinition moduleDefinition)
    {
        this.moduleDefinition = moduleDefinition;
    }

    public void Execute()
    {
        AllTypes = new List<TypeDefinition>();
        GetTypes(moduleDefinition.Types);
    }

    void GetTypes(IEnumerable<TypeDefinition> typeDefinitions)
    {
        foreach (var typeDefinition in typeDefinitions)
        {
            GetTypes(typeDefinition.NestedTypes);
            AllTypes.Add(typeDefinition);
        }
    }

}