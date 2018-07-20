using Fody;
using Mono.Cecil;

public partial class ModuleWeaver
{
    public MethodReference TraceWriteLineMethod;
    public MethodReference StartNewMethod;
    public MethodReference StopMethod;
    public TypeReference StopwatchType;
    public MethodReference StringFormatWithArray;
    public MethodReference ConcatMethod;
    public MethodReference Elapsed;
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
        if (!TryFindType("System.Diagnostics.Trace", out var traceType))
        {
            if (!TryFindType("System.Diagnostics.Debug", out traceType))
            {
                throw new WeavingException("Could not find either Trace Or Debug");
            }
        }

        var writeLine = traceType.Method("WriteLine", "String");
        TraceWriteLineMethod = ModuleDefinition.ImportReference(writeLine);

        var objectConstructor = FindType("System.Object").Method(".ctor");
        ObjectConstructorMethod = ModuleDefinition.ImportReference(objectConstructor);

        var mathType = FindType("System.Math");
        MaxMethod = ModuleDefinition.ImportReference(mathType.Method("Max", "Int64", "Int64"));

        var dateTimeType = FindType("System.DateTime");
        DateTimeType = ModuleDefinition.ImportReference(dateTimeType);
        UtcNowMethod = ModuleDefinition.ImportReference(dateTimeType.Method("get_UtcNow"));
        GetTicksMethod = ModuleDefinition.ImportReference(dateTimeType.Method("get_Ticks"));

        var methodBaseType = FindType("System.Reflection.MethodBase");
        var methodBase = methodBaseType.Method("GetMethodFromHandle", "RuntimeMethodHandle", "RuntimeTypeHandle");
        GetMethodFromHandle = ModuleDefinition.ImportReference(methodBase);

        var booleanType = FindType("System.Boolean");
        BooleanType = ModuleDefinition.ImportReference(booleanType);

        if (TryFindType("System.Diagnostics.Stopwatch", out var stopwatchType))
        {
            StopwatchType = ModuleDefinition.ImportReference(stopwatchType);
            StartNewMethod = ModuleDefinition.ImportReference(stopwatchType.Method("StartNew"));
            StopMethod = ModuleDefinition.ImportReference(stopwatchType.Method("Stop"));
            Elapsed = ModuleDefinition.ImportReference(stopwatchType.Method("get_Elapsed"));
            ElapsedMilliseconds = ModuleDefinition.ImportReference(stopwatchType.Method("get_ElapsedMilliseconds"));
        }
        else
        {
            // Note: injected stopwatch is not supported for TimeSpan elapsed, should we error or add?
            InjectStopwatchType();
        }

        var formatMethod = TypeSystem.StringDefinition.Method("Format", "String", "Object[]");
        StringFormatWithArray = ModuleDefinition.ImportReference(formatMethod);
        var concatMethod = TypeSystem.StringDefinition.Method("Concat", "Object", "Object", "Object");
        ConcatMethod = ModuleDefinition.ImportReference(concatMethod);
    }
}