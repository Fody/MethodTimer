using System.Linq;
using Fody;
using Mono.Cecil;

public partial class ModuleWeaver
{
    public MethodReference TraceWriteLineMethod;
    public TypeReference StopwatchType;
    public MethodReference StringFormatWithArray;
    public MethodReference ConcatMethod;
    public MethodReference GetMethodFromHandle;
    public MethodReference Stopwatch_GetTimestampMethod;
    public FieldReference Stopwatch_GetFrequencyField;
    public MethodReference TimeSpan_ConstructorMethod;
    public MethodReference TimeSpan_TotalMillisecondsMethod;
    public FieldDefinition MethodTimerHelper_TimestampToTicks;
    public TypeReference TimeSpanType;
    public TypeReference BooleanType;
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

        var timeSpanDefinition = FindTypeDefinition("System.TimeSpan");
        TimeSpanType = ModuleDefinition.ImportReference(timeSpanDefinition);
        var timeSpanConstructor = timeSpanDefinition.Method(".ctor","Int64");
        TimeSpan_ConstructorMethod = ModuleDefinition.ImportReference(timeSpanConstructor);

        var timeSpanTotalMilliseconds = timeSpanDefinition.Method("get_TotalMilliseconds");
        TimeSpan_TotalMillisecondsMethod = ModuleDefinition.ImportReference(timeSpanTotalMilliseconds);

        var methodBaseType = FindTypeDefinition("System.Reflection.MethodBase");
        var methodBase = methodBaseType.Method("GetMethodFromHandle", "RuntimeMethodHandle", "RuntimeTypeHandle");
        GetMethodFromHandle = ModuleDefinition.ImportReference(methodBase);

        var int64ToString = TypeSystem.Int64Definition.Method("ToString");
        Int64ToString = ModuleDefinition.ImportReference(int64ToString);

        var formatMethod = TypeSystem.StringDefinition.Method("Format", "String", "Object[]");
        StringFormatWithArray = ModuleDefinition.ImportReference(formatMethod);
        var concatMethod = TypeSystem.StringDefinition.Method("Concat", "String", "String", "String");
        ConcatMethod = ModuleDefinition.ImportReference(concatMethod);

        var stopwatchType = FindTypeDefinition("System.Diagnostics.Stopwatch");
        StopwatchType = ModuleDefinition.ImportReference(stopwatchType);
        if (StopwatchType is null)
        {
            throw new WeavingException("Could not find 'System.Diagnostics.Stopwatch', this seems to be an unsupported platform.");
        }

        var stopwatch_GetTimestampMethod = stopwatchType.Method("GetTimestamp");
        Stopwatch_GetTimestampMethod = ModuleDefinition.ImportReference(stopwatch_GetTimestampMethod);

        var stopwatch_GetFrequencyField = stopwatchType.Fields.First(x => x.Name == "Frequency");
        Stopwatch_GetFrequencyField = ModuleDefinition.ImportReference(stopwatch_GetFrequencyField);

        InjectMethodTimerHelper();
    }
}
