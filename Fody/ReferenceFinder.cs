using System.Linq;
using Mono.Cecil;

public class ReferenceFinder
{
    ModuleWeaver moduleWeaver;
    IAssemblyResolver assemblyResolver;
    public MethodReference WriteLineMethod;
    public MethodReference StartNewMethod;
    public MethodReference StopMethod;
    public TypeReference StopwatchType;
    public MethodReference ConcatMethod;
    public MethodReference ElapsedMilliseconds;
    

    public ReferenceFinder(ModuleWeaver moduleWeaver)
    {
        this.moduleWeaver = moduleWeaver;
        assemblyResolver = moduleWeaver.AssemblyResolver;
    }

    public void Execute()
    {
        var systemDefinition = assemblyResolver.Resolve("System");
        var systemTypes = systemDefinition.MainModule.Types;

        var module = moduleWeaver.ModuleDefinition;

        var debugType = systemTypes.First(x => x.Name == "Debug");
        WriteLineMethod = module.Import(debugType.Methods.First(x => 
            x.Name == "WriteLine" && 
            x.Parameters.Count == 1 && 
            x.Parameters[0].ParameterType.Name == "String"));

        var stopwatchType = systemTypes.First(x => x.Name == "Stopwatch");
        StopwatchType = module.Import(stopwatchType);
        StartNewMethod = module.Import(stopwatchType.Methods.First(x => x.Name == "StartNew"));
        StopMethod = module.Import(stopwatchType.Methods.First(x => x.Name == "Stop"));
        ElapsedMilliseconds = module.Import(stopwatchType.Methods.First(x => x.Name == "get_ElapsedMilliseconds"));

        var stringType = module.TypeSystem.String;
        ConcatMethod = module.Import(stringType.Resolve().Methods.First(x => x.Name == "Concat" && x.Parameters.Count == 3));


    }

}