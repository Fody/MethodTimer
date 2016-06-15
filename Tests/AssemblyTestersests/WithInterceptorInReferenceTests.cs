using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

[TestFixture]
public class WithInterceptorInReferenceTests
{
    AssemblyWeaver assemblyWeaver;
    FieldInfo methodBaseField;

    public WithInterceptorInReferenceTests()
    {
        var assemblyPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\AssemblyWIthInterceptorInReference\bin\Debug\AssemblyWIthInterceptorInReference.dll");
        var assemblyToReferencePath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\AssemblyToReference\bin\Debug\AssemblyToReference.dll");
        var assemblyToReference = AssemblyWeaver.FixAssemblyPath(assemblyToReferencePath);
        assemblyWeaver = new AssemblyWeaver(assemblyPath, new List<string> { assemblyToReference });

        var interceptorAssembly = Assembly.LoadFrom(assemblyToReference);
        var methodTimeLogger = interceptorAssembly.GetType("MethodTimeLogger");
        methodBaseField = methodTimeLogger.GetField("MethodBase");
    }

    [Test]
    public void ClassWithMethod()
    {
        ClearMessage();
        var type = assemblyWeaver.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Method();
        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);
        var methodBase = methodBases.First();
        Assert.AreEqual(methodBase.Name, "Method");
        Assert.AreEqual(methodBase.DeclaringType, type);
    }

    void ClearMessage()
    {
        methodBaseField.SetValue(null, new List<MethodBase>());
    }

    List<MethodBase> GetMethodInfoField()
    {
        return (List<MethodBase>)methodBaseField.GetValue(null);
    }

    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assemblyWeaver.Assembly.CodeBase.Remove(0, 8));
    }

}