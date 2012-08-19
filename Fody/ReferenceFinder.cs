using System.Linq;
using Mono.Cecil;

public class ReferenceFinder
{
    public ModuleDefinition ModuleDefinition;
    public IAssemblyResolver AssemblyResolver;
    public MethodReference DebugWriteLineMethod;
    public MethodReference StartNewMethod;
    public MethodReference StopMethod;
    public TypeReference StopwatchType;
    public MethodReference ConcatMethod;
    public MethodReference ElapsedMilliseconds;

    public MethodReference GetMethodFromHandle;
    

    
    public void Execute()
    {
        var systemDefinition = AssemblyResolver.Resolve("System");
        var systemTypes = systemDefinition.MainModule.Types;

        
        var debugType = systemTypes.First(x => x.Name == "Debug");
        DebugWriteLineMethod = ModuleDefinition.Import(debugType.Methods.First(x => 
            x.Name == "WriteLine" && 
            x.Parameters.Count == 1 && 
            x.Parameters[0].ParameterType.Name == "String"));



        var mscorlib = AssemblyResolver.Resolve("mscorlib");
        var mscorlibTypes = mscorlib.MainModule.Types;
        var methodBaseType = mscorlibTypes.First(x => x.Name == "MethodBase");
        GetMethodFromHandle = ModuleDefinition.Import(methodBaseType.Methods.First(x =>
            x.Name == "GetMethodFromHandle" &&
            x.Parameters.Count == 1 &&
            x.Parameters[0].ParameterType.Name == "RuntimeMethodHandle"));

        var stopwatchType = systemTypes.First(x => x.Name == "Stopwatch");
        StopwatchType = ModuleDefinition.Import(stopwatchType);
        StartNewMethod = ModuleDefinition.Import(stopwatchType.Methods.First(x => x.Name == "StartNew"));
        StopMethod = ModuleDefinition.Import(stopwatchType.Methods.First(x => x.Name == "Stop"));
        ElapsedMilliseconds = ModuleDefinition.Import(stopwatchType.Methods.First(x => x.Name == "get_ElapsedMilliseconds"));

        var stringType = ModuleDefinition.TypeSystem.String;
        ConcatMethod = ModuleDefinition.Import(stringType.Resolve().Methods.First(x => x.Name == "Concat" && x.Parameters.Count == 3));


    }

}