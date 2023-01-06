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
    public MethodReference IsRunning;
    public MethodReference Elapsed;
    public MethodReference ElapsedMilliseconds;
    public MethodReference GetMethodFromHandle;
    public MethodReference ObjectConstructorMethod;
    public MethodReference MaxMethod;
    public MethodReference GetTicksMethod;
    public MethodReference UtcNowMethod;
    public TypeReference DateTimeType;

    public void FindReferences()
    {
        if (!TryFindTypeDefinition("System.Diagnostics.Trace", out var traceType))
        {
            if (!TryFindTypeDefinition("System.Diagnostics.Debug", out traceType))
            {
                throw new WeavingException("Could not find either Trace Or Debug");
            }
        }

        var writeLine = traceType.Method("WriteLine", "String");
        TraceWriteLineMethod = ModuleDefinition.ImportReference(writeLine);

        var objectConstructor = TypeSystem.ObjectDefinition.Method(".ctor");
        ObjectConstructorMethod = ModuleDefinition.ImportReference(objectConstructor);

        var mathType = FindTypeDefinition("System.Math");
        MaxMethod = ModuleDefinition.ImportReference(mathType.Method("Max", "Int64", "Int64"));

        var dateTimeType = FindTypeDefinition("System.DateTime");
        DateTimeType = ModuleDefinition.ImportReference(dateTimeType);
        UtcNowMethod = ModuleDefinition.ImportReference(dateTimeType.Method("get_UtcNow"));
        GetTicksMethod = ModuleDefinition.ImportReference(dateTimeType.Method("get_Ticks"));

        var methodBaseType = FindTypeDefinition("System.Reflection.MethodBase");
        var methodBase = methodBaseType.Method("GetMethodFromHandle", "RuntimeMethodHandle", "RuntimeTypeHandle");
        GetMethodFromHandle = ModuleDefinition.ImportReference(methodBase);

        var formatMethod = TypeSystem.StringDefinition.Method("Format", "String", "Object[]");
        StringFormatWithArray = ModuleDefinition.ImportReference(formatMethod);
        var concatMethod = TypeSystem.StringDefinition.Method("Concat", "Object", "Object", "Object");
        ConcatMethod = ModuleDefinition.ImportReference(concatMethod);

        if (TryFindTypeDefinition("System.Diagnostics.Stopwatch", out var stopwatchType))
        {
            StopwatchType = ModuleDefinition.ImportReference(stopwatchType);
            StartNewMethod = ModuleDefinition.ImportReference(stopwatchType.Method("StartNew"));
            StopMethod = ModuleDefinition.ImportReference(stopwatchType.Method("Stop"));
            Elapsed = ModuleDefinition.ImportReference(stopwatchType.Method("get_Elapsed"));
            ElapsedMilliseconds = ModuleDefinition.ImportReference(stopwatchType.Method("get_ElapsedMilliseconds"));
            IsRunning = ModuleDefinition.ImportReference(stopwatchType.Method("get_IsRunning"));
        }
        else
        {
            // Note: injected stopwatch is not supported for TimeSpan elapsed, should we error or add?
            InjectStopwatchType();
        }
    }
}