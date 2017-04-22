using System.Diagnostics;
using Mono.Cecil;

public class MockAssemblyResolver : DefaultAssemblyResolver
{
    public override AssemblyDefinition Resolve(AssemblyNameReference name)
    {
        if (name.Name == "System")
        {
            var codeBase = typeof(Debug).Assembly.CodeBase.Replace("file:///", "");
            return AssemblyDefinition.ReadAssembly(codeBase);
        }
        else
        {
            var codeBase = typeof(string).Assembly.CodeBase.Replace("file:///", "");
            return AssemblyDefinition.ReadAssembly(codeBase);
        }

    }

}