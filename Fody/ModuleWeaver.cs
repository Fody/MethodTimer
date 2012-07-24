using System;
using System.Xml.Linq;
using Mono.Cecil;

public class ModuleWeaver
{
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public IAssemblyResolver AssemblyResolver { get; set; }
    public XElement Config { get; set; }

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
    }

    public void Execute()
    {
        var allTypesFinder = new AllTypesFinder(ModuleDefinition);
        allTypesFinder.Execute();

        var attributeFixer = new MethodProcessor(ModuleDefinition);
        var assemblyProcessor = new AssemblyProcessor(allTypesFinder, attributeFixer);
        assemblyProcessor.Execute();

    }
}