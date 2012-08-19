using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

[TestFixture]
public class WithInterceptorTests
{
    Assembly assembly;
    FieldInfo methodBaseField;

    public WithInterceptorTests()
    {
        var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\DebugWithInterceptor\AssemblyToProcess.dll");
        assembly = AssemblyWeaver.Weave(assemblyPath);
        var methodTimeLogger = assembly.GetType("MethodTimeLogger");
        methodBaseField = methodTimeLogger.GetField("MethodBase");
    }

    [Test]
    public void ClassWithAttribute()
    {
        ClearMessage();
        var type = assembly.GetType("ClassWithAttribute");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();
        Assert.AreEqual(GetMethodInfoField().Name, "Method");
        Assert.AreEqual(GetMethodInfoField().DeclaringType, type);
    }
    [Test]
    public void ClassWithConstructor()
    {
        ClearMessage();
        var type = assembly.GetType("ClassWithConstructor");
        Activator.CreateInstance(type);
        Assert.AreEqual(GetMethodInfoField().Name, ".ctor");
        Assert.AreEqual(GetMethodInfoField().DeclaringType, type);
    }

    [Test]
    public void ClassWithMethod()
    {
        ClearMessage();
        var type = assembly.GetType("ClassWithMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();
        Assert.AreEqual(GetMethodInfoField().Name, "Method");
        Assert.AreEqual(GetMethodInfoField().DeclaringType, type);

    }

    [Test]
    public void MethodWithReturn()
    {
        ClearMessage();
        var type = assembly.GetType("MiscMethods");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.MethodWithReturn();
        Assert.AreEqual(GetMethodInfoField().Name, "MethodWithReturn");
        Assert.AreEqual(GetMethodInfoField().DeclaringType, type);
    }


    void ClearMessage()
    {
        methodBaseField.SetValue(null, null);
    }

    MethodBase GetMethodInfoField()
    {
        return (MethodBase)methodBaseField.GetValue(null);
    }

#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assembly.CodeBase.Remove(0, 8));
    }
#endif

}