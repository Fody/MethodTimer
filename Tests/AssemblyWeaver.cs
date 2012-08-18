using System.IO;
using System.Reflection;
using Mono.Cecil;

public static class AssemblyWeaver
{
    public static Assembly Weave(string assemblyPath)
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
            };

        weavingTask.Execute();
        moduleDefinition.Write(newAssembly);

        return Assembly.LoadFile(newAssembly);
    }


}