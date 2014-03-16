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
    public List<string> ReferenceCopyLocalPaths { get; set; }

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogError = s => { };
        ReferenceCopyLocalPaths = new List<string>();
    }

    public void Execute()
    {
        types = ModuleDefinition.GetTypes().ToList();
        FindReferences();
        FindInterceptor();
        CheckForBadAttributes();
        ProcessAssembly();
        RemoveAttributes();
        RemoveReference();
    }

}