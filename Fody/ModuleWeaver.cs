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
    ReferenceFinder msCoreReferenceFinder;
    InterceptorFinder interceptorFinder;

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
    }

    public void Execute()
    {
        msCoreReferenceFinder = new ReferenceFinder
            {
                ModuleDefinition = ModuleDefinition,
                AssemblyResolver = AssemblyResolver
            };
        msCoreReferenceFinder.Execute();

        interceptorFinder = new InterceptorFinder
            {
                ModuleDefinition = ModuleDefinition
            };
        interceptorFinder.Execute();

        var types = ModuleDefinition.GetTypes().ToList();
        foreach (var typeDefinition in types)
        {
            if (typeDefinition.ContainsTimeAttribute())
            {
                foreach (var method in typeDefinition.Methods.Where(x => !x.IsAbstract))
                {
                    ProcessMethod(method);
                }
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
                ProcessMethod(method);
            }
        }

        foreach (var timeAttribute in ModuleDefinition.Types
            .Where(x => !x.IsPublic && x.Name == "TimeAttribute")
            .ToList())
        {
            ModuleDefinition.Types.Remove(timeAttribute);
        }
    }

    void ProcessMethod(MethodDefinition method)
    {
        var methodProcessor = new MethodProcessor
            {
                ReferenceFinder = msCoreReferenceFinder,
                TypeSystem = ModuleDefinition.TypeSystem,
                InterceptorFinder = interceptorFinder,
                Method = method,
            };
        methodProcessor.Process();
    }
}