using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{
    public MethodReference DebugWriteLineMethod;
    public MethodReference StartNewMethod;
    public MethodReference StopMethod;
    public TypeReference StopwatchType;
    public MethodReference StringFormatWithArray;
    public MethodReference ConcatMethod;
    public MethodReference ElapsedMilliseconds;
    public MethodReference GetMethodFromHandle;
    public MethodReference ObjectConstructorMethod;
    public MethodReference MaxMethod;
    public MethodReference GetTicksMethod;
    public MethodReference UtcNowMethod;
    public TypeReference DateTimeType;
    public TypeReference BooleanType;
    
    public void FindReferences()
    {
        var refTypes = new List<TypeDefinition>();
        AddAssemblyIfExists("System.Runtime.Extensions", refTypes);
        AddAssemblyIfExists("System", refTypes);
        AddAssemblyIfExists("mscorlib", refTypes);
        AddAssemblyIfExists("System.Runtime", refTypes);
        AddAssemblyIfExists("System.Reflection", refTypes);
        AddAssemblyIfExists("System.Diagnostics.Debug", refTypes);

        var debugType = refTypes.First(x => x.Name == "Debug");

        DebugWriteLineMethod = ModuleDefinition.ImportReference(debugType.Methods.First(x => 
            x.Name == "WriteLine" && 
            x.Parameters.Count == 1 && 
            x.Parameters[0].ParameterType.Name == "String"));

        ObjectConstructorMethod = ModuleDefinition.ImportReference(refTypes.First(x=>x.Name=="Object").Methods.First(x => x.Name == ".ctor"));

        var mathType = refTypes.First(x => x.Name == "Math");
        MaxMethod = ModuleDefinition.ImportReference(mathType.Methods.First(x => 
            x.Name == "Max" && 
            x.Parameters[0].ParameterType.Name == "Int64"));

        var dateTimeType = refTypes.First(x => x.Name == "DateTime");
        DateTimeType = ModuleDefinition.ImportReference(dateTimeType);
        UtcNowMethod = ModuleDefinition.ImportReference(dateTimeType.Methods.First(x => x.Name == "get_UtcNow"));
        GetTicksMethod = ModuleDefinition.ImportReference(dateTimeType.Methods.First(x => x.Name == "get_Ticks"));

        var methodBaseType = refTypes.First(x => x.Name == "MethodBase");
        GetMethodFromHandle = ModuleDefinition.ImportReference(methodBaseType.Methods.First(x =>
            x.Name == "GetMethodFromHandle" &&
            x.Parameters.Count == 2 &&
            x.Parameters[0].ParameterType.Name == "RuntimeMethodHandle" &&
            x.Parameters[1].ParameterType.Name == "RuntimeTypeHandle"));

        var booleanType = refTypes.First(x => x.Name == "Boolean");
        BooleanType = ModuleDefinition.ImportReference(booleanType);

        var stopwatchType = refTypes.FirstOrDefault(x => x.Name == "Stopwatch");
        if (stopwatchType == null)
        {
            InjectStopwatchType();
        }
        else
        {
            StopwatchType = ModuleDefinition.ImportReference(stopwatchType);
            StartNewMethod = ModuleDefinition.ImportReference(stopwatchType.Methods.First(x => x.Name == "StartNew"));
            StopMethod = ModuleDefinition.ImportReference(stopwatchType.Methods.First(x => x.Name == "Stop"));
            ElapsedMilliseconds = ModuleDefinition.ImportReference(stopwatchType.Methods.First(x => x.Name == "get_ElapsedMilliseconds"));   
        }

        var stringType = ModuleDefinition.TypeSystem.String.Resolve();
        StringFormatWithArray = ModuleDefinition.ImportReference(stringType.Methods.First(x =>
            x.Name == "Format" &&
            x.Parameters.Count == 2 &&
            x.Parameters[0].ParameterType.Name == "String" &&
            x.Parameters[1].ParameterType.Name == "Object[]"));
        ConcatMethod = ModuleDefinition.ImportReference(stringType.Methods.First(x => x.Name == "Concat" && x.Parameters.Count == 3));
    }

    void AddAssemblyIfExists(string name, List<TypeDefinition> refTypes)
    {
        try
        {
            var assembly = AssemblyResolver.Resolve(new AssemblyNameReference(name, null));

            if (assembly != null)
            {
                refTypes.AddRange(assembly.MainModule.Types);
            }
        }
        catch (AssemblyResolutionException)
        {
            LogInfo($"Failed to resolve '{name}'. So skipping its types.");
        }
    }
}