using System;
using System.Reflection;
using Mono.Cecil;

public class MockAssemblyResolver : IAssemblyResolver
{
    public AssemblyDefinition Resolve(AssemblyNameReference name)
    {
        throw new NotImplementedException();
    }

    public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
    {
        throw new NotImplementedException();
    }

    public AssemblyDefinition Resolve(string fullName)
    {
        var codeBase = Assembly.Load(fullName).CodeBase.Replace("file:///","");

        return AssemblyDefinition.ReadAssembly(codeBase);
    }

    public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
    {
        throw new NotImplementedException();
    }
}