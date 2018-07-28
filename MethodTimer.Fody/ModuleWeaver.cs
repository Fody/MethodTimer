using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;

public partial class ModuleWeaver: BaseModuleWeaver
{
    List<TypeDefinition> types;

    ParameterFormattingProcessor parameterFormattingProcessor = new ParameterFormattingProcessor();

    public ModuleWeaver()
    {
        ReferenceCopyLocalPaths = new List<string>();
    }

    public override void Execute()
    {
        types = ModuleDefinition.GetTypes().ToList();
        FindReferences();
        FindInterceptor();
        if (LogMethodIsNop)
        {
            var logMethod = GetPreferredLogMethod();
            LogDebug($"'{logMethod?.FullName}' is a Nop so skipping weaving");
            RemoveAttributes();
            return;
        }
        CheckForBadAttributes();
        ProcessAssembly();
        RemoveAttributes();
    }

    MethodReference GetPreferredLogMethod()
    {
        // TimeSpan first, then long
        return LogWithMessageMethodUsingTimeSpan ?? LogMethodUsingTimeSpan ?? LogWithMessageMethodUsingLong ?? LogMethodUsingLong;
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "System.Runtime.Extensions";
        yield return "System";
        yield return "mscorlib";
        yield return "System.Diagnostics.TraceSource";
        yield return "System.Diagnostics.Debug";
        yield return "System.Runtime";
        yield return "System.Reflection";
        yield return "netstandard";
    }

    public override bool ShouldCleanReference => true;
}