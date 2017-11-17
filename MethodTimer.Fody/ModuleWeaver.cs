using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    public Action<string> LogDebug { get; set; }
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public Action<string, SequencePoint> LogWarningPoint { get; set; }
    public Action<string> LogError { get; set; }
    public Action<string, SequencePoint> LogErrorPoint { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public IAssemblyResolver AssemblyResolver { get; set; }
    List<TypeDefinition> types;
    public List<string> ReferenceCopyLocalPaths { get; set; }

    ParameterFormattingProcessor parameterFormattingProcessor = new ParameterFormattingProcessor();

    public ModuleWeaver()
    {
        LogDebug = s => { Trace.WriteLine(s); };
        LogInfo = s => { Trace.WriteLine(s); };
        LogWarning = s => { Trace.WriteLine(s); };
        LogWarningPoint = (s, p) => { Trace.WriteLine(s); };
        LogError = s => { Trace.WriteLine(s); };
        LogErrorPoint = (s, p) => { Trace.WriteLine(s); };
        ReferenceCopyLocalPaths = new List<string>();
    }

    public void Execute()
    {
        types = ModuleDefinition.GetTypes().ToList();
        FindReferences();
        FindInterceptor();
        if (LogMethodIsNop)
        {
            var logMethod = LogMethod ?? LogWithMessageMethod;
            LogDebug($"'{logMethod?.FullName}' is a Nop so skipping weaving");
            RemoveAttributes();
            RemoveReference();
            return;
        }
        CheckForBadAttributes();
        ProcessAssembly();
        RemoveAttributes();
        RemoveReference();
    }
}