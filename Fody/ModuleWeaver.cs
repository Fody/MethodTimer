using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{
    public Action<string> LogInfo { get; set; }
    public Action<string> LogError { get; set; }
    public Action<string> LogWarning { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public IAssemblyResolver AssemblyResolver { get; set; }
    List<TypeDefinition> types;

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogError = s => { };
    }

    public void Execute()
    {
        FindReferences();
        types = ModuleDefinition.GetTypes().ToList();
        FindInterceptor();
        CheckForBadAttributes();
        ProcessAssembly();
        RemoveAttributes();
        RemoveReference();
    }

}