using System.Collections.Generic;
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
    public MethodReference ObjectConstructorMethod;
    public MethodReference MaxMethod;
    public MethodReference GetTicksMethod;
    public MethodReference UtcNowMethod;
    public TypeReference DateTimeType;
    
    public void FindReferences()
    {
        var coreTypes = new List<TypeDefinition>();
        AppendTypes("System.Runtime.Extensions", coreTypes);
        AppendTypes("System", coreTypes);
        AppendTypes("mscorlib", coreTypes);
        AppendTypes("System.Runtime", coreTypes);
        AppendTypes("System.Reflection", coreTypes);

        var debugType = GetDebugType(coreTypes);

        DebugWriteLineMethod = ModuleDefinition.Import(debugType.Methods.First(x => 
            x.Name == "WriteLine" && 
            x.Parameters.Count == 1 && 
            x.Parameters[0].ParameterType.Name == "String"));

        ObjectConstructorMethod = ModuleDefinition.Import(coreTypes.First(x=>x.Name=="Object").Methods.First(x => x.Name == ".ctor"));

        var mathType = coreTypes.First(x => x.Name == "Math");
        MaxMethod = ModuleDefinition.Import(mathType.Methods.First(x => 
            x.Name == "Max" && 
            x.Parameters[0].ParameterType.Name == "Int64"));

        var dateTimeType = coreTypes.First(x => x.Name == "DateTime");
        DateTimeType = ModuleDefinition.Import(dateTimeType);
        UtcNowMethod = ModuleDefinition.Import(dateTimeType.Methods.First(x => x.Name == "get_UtcNow"));
        GetTicksMethod = ModuleDefinition.Import(dateTimeType.Methods.First(x => x.Name == "get_Ticks"));

        var methodBaseType = coreTypes.First(x => x.Name == "MethodBase");
        GetMethodFromHandle = ModuleDefinition.Import(methodBaseType.Methods.First(x =>
            x.Name == "GetMethodFromHandle" &&
            x.Parameters.Count == 1 &&
            x.Parameters[0].ParameterType.Name == "RuntimeMethodHandle"));

        var stopwatchType = coreTypes.FirstOrDefault(x => x.Name == "Stopwatch");
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

    void AppendTypes(string name, List<TypeDefinition> coreTypes)
    {
        var definition = AssemblyResolver.Resolve(name);
        if (definition != null)
        {
            coreTypes.AddRange(definition.MainModule.Types);
        }
    }

    TypeDefinition GetDebugType(List<TypeDefinition> coreTypes)
    {
        var debugType = coreTypes.FirstOrDefault(x => x.Name == "Debug");

        if (debugType != null)
        {
            return debugType;
        }
        var systemDiagnosticsDebug = AssemblyResolver.Resolve("System.Diagnostics.Debug");
        if (systemDiagnosticsDebug != null)
        {
            debugType = systemDiagnosticsDebug.MainModule.Types.FirstOrDefault(x => x.Name == "Debug");
            if (debugType != null)
            {
                return debugType;
            }
        }

        throw new WeavingException("Could not find the 'Debug' type. PLease raise an issue and include the wacky version of .net MS has forced upon you.");
    }

}