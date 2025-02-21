using System.Linq;
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
    public MethodReference Stopwatch_GetTimestampMethod;
    public FieldReference Stopwatch_GetFrequencyField;
    public MethodReference UtcNowMethod;
    public MethodReference TimeSpan_ConstructorMethod;
    public MethodReference TimeSpan_TotalMillisecondsMethod;
    public FieldReference TimeSpan_TicksPerSecondField;
    public FieldDefinition MethodTimerHelper_TimestampToTicks;
    public TypeReference DateTimeType;
    public TypeReference TimeSpanType;
    public TypeReference BooleanType;
    public TypeReference VoidType;
    public TypeReference Float64Type;
    public TypeReference MethodTimerHelperType;
    public MethodReference Int64ToString;

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

        var voidType = FindTypeDefinition("System.Void");
        VoidType = ModuleDefinition.ImportReference(voidType);

        var timeSpanDefinition = FindTypeDefinition("System.TimeSpan");
        TimeSpanType = ModuleDefinition.ImportReference(timeSpanDefinition);
        var timeSpanConstructor = timeSpanDefinition.Method(".ctor","Int64");
        TimeSpan_ConstructorMethod = ModuleDefinition.ImportReference(timeSpanConstructor);

        var timeSpanTotalMilliseconds = timeSpanDefinition.Method("get_TotalMilliseconds");
        TimeSpan_TotalMillisecondsMethod = ModuleDefinition.ImportReference(timeSpanTotalMilliseconds);

        var timeSpanTicksPerSecond = timeSpanDefinition.Fields.First(x => x.Name == "TicksPerSecond");
        TimeSpan_TicksPerSecondField = ModuleDefinition.ImportReference(timeSpanTicksPerSecond);

        var mathType = FindTypeDefinition("System.Math");
        MaxMethod = ModuleDefinition.ImportReference(mathType.Method("Max", "Int64", "Int64"));

        var dateTimeType = FindTypeDefinition("System.DateTime");
        DateTimeType = ModuleDefinition.ImportReference(dateTimeType);
        UtcNowMethod = ModuleDefinition.ImportReference(dateTimeType.Method("get_UtcNow"));
        GetTicksMethod = ModuleDefinition.ImportReference(dateTimeType.Method("get_Ticks"));

        var methodBaseType = FindTypeDefinition("System.Reflection.MethodBase");
        var methodBase = methodBaseType.Method("GetMethodFromHandle", "RuntimeMethodHandle", "RuntimeTypeHandle");
        GetMethodFromHandle = ModuleDefinition.ImportReference(methodBase);

        var int64ToString = TypeSystem.Int64Definition.Method("ToString");
        Int64ToString = ModuleDefinition.ImportReference(int64ToString);

        var formatMethod = TypeSystem.StringDefinition.Method("Format", "String", "Object[]");
        StringFormatWithArray = ModuleDefinition.ImportReference(formatMethod);
        var concatMethod = TypeSystem.StringDefinition.Method("Concat", "String", "String", "String");
        ConcatMethod = ModuleDefinition.ImportReference(concatMethod);

        var float64Type = FindTypeDefinition("System.Double");
        Float64Type = ModuleDefinition.ImportReference(float64Type);

        var stopwatchType = FindTypeDefinition("System.Diagnostics.Stopwatch");
        StopwatchType = ModuleDefinition.ImportReference(stopwatchType);
        if (StopwatchType is null)
        {
            throw new WeavingException($"Could not find 'System.Diagnostics.Stopwatch', this seems to be an unsupported platform.");
        }

        var startNewMethod = stopwatchType.Method("StartNew");
        StartNewMethod = ModuleDefinition.ImportReference(startNewMethod);

        var stopMethod = stopwatchType.Method("Stop");
        StopMethod = ModuleDefinition.ImportReference(stopMethod);

        var getElapsedMillisecondsMethod = stopwatchType.Method("get_ElapsedMilliseconds");
        ElapsedMilliseconds = ModuleDefinition.ImportReference(getElapsedMillisecondsMethod);

        var getElapsedMethod = stopwatchType.Method("get_Elapsed");
        Elapsed = ModuleDefinition.ImportReference(getElapsedMethod);

        var getIsRunning = stopwatchType.Method("get_IsRunning");
        IsRunning = ModuleDefinition.ImportReference(getIsRunning);

        var stopwatch_GetTimestampMethod = stopwatchType.Method("GetTimestamp");
        Stopwatch_GetTimestampMethod = ModuleDefinition.ImportReference(stopwatch_GetTimestampMethod);

        var stopwatch_GetFrequencyField = stopwatchType.Fields.First(x => x.Name == "Frequency");
        Stopwatch_GetFrequencyField = ModuleDefinition.ImportReference(stopwatch_GetFrequencyField);

        InjectMethodTimerHelper();
    }
}