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
        AddAssemblyIfExists("netstandard", refTypes);

        var debugType = refTypes.Type("Debug");

        var writeLine = debugType.Method("WriteLine", "String");
        DebugWriteLineMethod = ModuleDefinition.ImportReference(writeLine);

        var objectConstructor = refTypes.Type("Object").Method(".ctor");
        ObjectConstructorMethod = ModuleDefinition.ImportReference(objectConstructor);

        var mathType = refTypes.Type("Math");
        MaxMethod = ModuleDefinition.ImportReference(mathType.Method("Max", "Int64", "Int64"));

        var dateTimeType = refTypes.Type("DateTime");
        DateTimeType = ModuleDefinition.ImportReference(dateTimeType);
        UtcNowMethod = ModuleDefinition.ImportReference(dateTimeType.Method("get_UtcNow"));
        GetTicksMethod = ModuleDefinition.ImportReference(dateTimeType.Method("get_Ticks"));

        var methodBaseType = refTypes.Type("MethodBase");
        var methodBase = methodBaseType.Method("GetMethodFromHandle", "RuntimeMethodHandle", "RuntimeTypeHandle");
        GetMethodFromHandle = ModuleDefinition.ImportReference(methodBase);

        var booleanType = refTypes.Type("Boolean");
        BooleanType = ModuleDefinition.ImportReference(booleanType);

        var stopwatchType = refTypes.FirstOrDefault(x => x.Name == "Stopwatch");
        if (stopwatchType == null)
        {
            InjectStopwatchType();
        }
        else
        {
            StopwatchType = ModuleDefinition.ImportReference(stopwatchType);
            StartNewMethod = ModuleDefinition.ImportReference(stopwatchType.Method("StartNew"));
            StopMethod = ModuleDefinition.ImportReference(stopwatchType.Method("Stop"));
            ElapsedMilliseconds = ModuleDefinition.ImportReference(stopwatchType.Method("get_ElapsedMilliseconds"));
        }

        var stringType = ModuleDefinition.TypeSystem.String.Resolve();
        var formatMethod = stringType.Method("Format", "String", "Object[]");
        StringFormatWithArray = ModuleDefinition.ImportReference(formatMethod);
        var concatMethod = stringType.Method("Concat", "Object", "Object", "Object");
        ConcatMethod = ModuleDefinition.ImportReference(concatMethod);
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