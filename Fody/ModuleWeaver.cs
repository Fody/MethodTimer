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
        var msCoreReferenceFinder = new ReferenceFinder
            {
                ModuleDefinition = ModuleDefinition,
                AssemblyResolver = AssemblyResolver
            };
        msCoreReferenceFinder.Execute();

        var interceptorFinder = new InterceptorFinder
            {
                ModuleDefinition = ModuleDefinition
            };
        interceptorFinder.Execute();

        var methodProcessor = new MethodProcessor
            {
                referenceFinder = msCoreReferenceFinder,
                typeSystem = ModuleDefinition.TypeSystem,
                InterceptorFinder = interceptorFinder,
            };
        var types = ModuleDefinition.GetTypes().ToList();
        foreach (var typeDefinition in types)
        {
            if (typeDefinition.ContainsTimeAttribute())
            {
                methodProcessor.Process(typeDefinition.Methods.Where(x => !x.IsAbstract));
                continue;
            }
            foreach (var method in typeDefinition.Methods)
            {
                if (method.IsAbstract)
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

        foreach (var timeAttribute in ModuleDefinition.Types
            .Where(x => !x.IsPublic && x.Name == "TimeAttribute")
            .ToList())
        {
            ModuleDefinition.Types.Remove(timeAttribute);
        }
    }

}