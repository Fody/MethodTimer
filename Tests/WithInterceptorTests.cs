using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

[TestFixture]
public class WithInterceptorTests
{
    Assembly assembly;

    public WithInterceptorTests()
    {
        var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\DebugWithInterceptor\AssemblyToProcess.dll");
        assembly = AssemblyWeaver.Weave(assemblyPath);
        var methodTimeLogger = assembly.GetType("MethodTimeLogger");
        methodInfoField = methodTimeLogger.GetField("MethodInfo");
    }

    FieldInfo methodInfoField;

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
        methodInfoField.SetValue(null, null);
    }

    MethodInfo GetMethodInfoField()
    {
        return (MethodInfo)methodInfoField.GetValue(null);
    }

#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assembly.CodeBase.Remove(0, 8));
    }
#endif

}