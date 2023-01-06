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
    public MethodReference TimeSpanConstructorMethod;
    public MethodReference MaxMethod;
    public MethodReference GetTicksMethod;
    public MethodReference UtcNowMethod;
    public TypeReference DateTimeType;
    public TypeReference TimeSpanType;
    public TypeReference BooleanType;
    public TypeReference VoidType;

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

        var objectConstructor = FindTypeDefinition("System.Object").Method(".ctor");
        ObjectConstructorMethod = ModuleDefinition.ImportReference(objectConstructor);

        var timeSpanDefinition = FindTypeDefinition("System.TimeSpan");
        TimeSpanType = ModuleDefinition.ImportReference(timeSpanDefinition);
        var timeSpanConstructor = timeSpanDefinition.Method(".ctor","Int64");
        TimeSpanConstructorMethod = ModuleDefinition.ImportReference(timeSpanConstructor);

        var mathType = FindTypeDefinition("System.Math");
        MaxMethod = ModuleDefinition.ImportReference(mathType.Method("Max", "Int64", "Int64"));

        var dateTimeType = FindTypeDefinition("System.DateTime");
        DateTimeType = ModuleDefinition.ImportReference(dateTimeType);
        UtcNowMethod = ModuleDefinition.ImportReference(dateTimeType.Method("get_UtcNow"));
        GetTicksMethod = ModuleDefinition.ImportReference(dateTimeType.Method("get_Ticks"));

        var methodBaseType = FindTypeDefinition("System.Reflection.MethodBase");
        var methodBase = methodBaseType.Method("GetMethodFromHandle", "RuntimeMethodHandle", "RuntimeTypeHandle");
        GetMethodFromHandle = ModuleDefinition.ImportReference(methodBase);

        var booleanType = FindTypeDefinition("System.Boolean");
        BooleanType = ModuleDefinition.ImportReference(booleanType);

        var voidType = FindTypeDefinition("System.Void");
        VoidType = ModuleDefinition.ImportReference(voidType);

        var formatMethod = TypeSystem.StringDefinition.Method("Format", "String", "Object[]");
        StringFormatWithArray = ModuleDefinition.ImportReference(formatMethod);
        var concatMethod = TypeSystem.StringDefinition.Method("Concat", "Object", "Object", "Object");
        ConcatMethod = ModuleDefinition.ImportReference(concatMethod);

        InjectStopwatchType();
    }
}