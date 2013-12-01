using System.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{
    public MethodReference DebugWriteLineMethod;
    public MethodReference StartNewMethod;
    public MethodReference StopMethod;
    public TypeReference StopwatchType;
    public MethodReference ConcatMethod;
    public MethodReference ElapsedMilliseconds;

    public MethodReference GetMethodFromHandle;
    

    
    public void FindReferences()
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

        var objectType = mscorlibTypes.First(x => x.Name == "Object");
        ObjectConstructorMethod = ModuleDefinition.Import(objectType.Methods.First(x => x.Name == ".ctor"));

        var mathType = mscorlibTypes.First(x => x.Name == "Math");
        MaxMethod = ModuleDefinition.Import(mathType.Methods.First(x => 
            x.Name == "Max" && 
            x.Parameters[0].ParameterType.Name == "Int64"));

        var dateTimeType = mscorlibTypes.First(x => x.Name == "DateTime");
        DateTimeType = ModuleDefinition.Import(dateTimeType);
        UtcNowMethod = ModuleDefinition.Import(dateTimeType.Methods.First(x => x.Name == "get_UtcNow"));
        GetTicksMethod = ModuleDefinition.Import(dateTimeType.Methods.First(x => x.Name == "get_Ticks"));



        var methodBaseType = mscorlibTypes.First(x => x.Name == "MethodBase");
        GetMethodFromHandle = ModuleDefinition.Import(methodBaseType.Methods.First(x =>
            x.Name == "GetMethodFromHandle" &&
            x.Parameters.Count == 1 &&
            x.Parameters[0].ParameterType.Name == "RuntimeMethodHandle"));

        var stopwatchType = systemTypes.FirstOrDefault(x => x.Name == "Stopwatch");
        if (stopwatchType == null)
        {
            InjectStopwatch();
        }
        else
        {
            StopwatchType = ModuleDefinition.Import(stopwatchType);
            StartNewMethod = ModuleDefinition.Import(stopwatchType.Methods.First(x => x.Name == "StartNew"));
            StopMethod = ModuleDefinition.Import(stopwatchType.Methods.First(x => x.Name == "Stop"));
            ElapsedMilliseconds = ModuleDefinition.Import(stopwatchType.Methods.First(x => x.Name == "get_ElapsedMilliseconds"));   
        }

        var stringType = ModuleDefinition.TypeSystem.String;
        ConcatMethod = ModuleDefinition.Import(stringType.Resolve().Methods.First(x => x.Name == "Concat" && x.Parameters.Count == 3));


    }

    public MethodReference ObjectConstructorMethod;

    public MethodReference MaxMethod;

    public MethodReference GetTicksMethod;

    public MethodReference UtcNowMethod;

    public TypeReference DateTimeType;
}