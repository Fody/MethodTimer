using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;

public class AssemblyWeaver
{
    public Assembly Assembly;
    public AssemblyWeaver(string assemblyPath)
    {
#if (!DEBUG)
        assemblyPath = assemblyPath.Replace("Debug", "Release");
#endif

        var newAssembly = assemblyPath.Replace(".dll", "2.dll");
        File.Copy(assemblyPath, newAssembly, true);

        var moduleDefinition = ModuleDefinition.ReadModule(newAssembly);
        var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyResolver = new MockAssemblyResolver(),
                LogError = LogError
            };

        weavingTask.Execute();
        moduleDefinition.Write(newAssembly);

        Assembly = Assembly.LoadFile(newAssembly);
    }

    void LogError(string error)
    {
        Errors.Add(error);
    }

    public List<string> Errors = new List<string>();
}