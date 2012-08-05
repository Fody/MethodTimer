using System;
using System.Linq;
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
        
        var msCoreReferenceFinder = new ReferenceFinder(this);
        msCoreReferenceFinder.Execute();
        var methodProcessor = new MethodProcessor(ModuleDefinition, msCoreReferenceFinder);
        foreach (var typeDefinition in ModuleDefinition.GetTypes())
        {
            if (typeDefinition.ContainsTimeAttribute())
            {
                methodProcessor.Process(typeDefinition.Methods.Where(x => x.IsMethodWithBody()));
                continue;
            }
            foreach (var method in typeDefinition.Methods)
            {
                if (!method.IsMethodWithBody())
                {
                    continue;
                }
                if (!method.ContainsTimeAttribute())
                {
                    continue;
                }
                methodProcessor.Process(method);
            }
        }
    }

}