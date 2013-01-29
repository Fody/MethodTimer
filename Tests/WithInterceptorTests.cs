using System;
using System.IO;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

[TestFixture]
public class WithInterceptorTests
{
    AssemblyWeaver assemblyWeaver;
    FieldInfo methodBaseField;

    public WithInterceptorTests()
    {
        var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\DebugWithInterceptor\AssemblyToProcess.dll");
        assemblyWeaver = new AssemblyWeaver(assemblyPath);
        var methodTimeLogger = assemblyWeaver.Assembly.GetType("MethodTimeLogger");
        methodBaseField = methodTimeLogger.GetField("MethodBase");
    }

    [Test]
    public void ErrorsForAbstract()
    {
        Assert.Contains("Method 'System.Void AbstractClassWithAttributeOnMethod::Method()' is abstract but has a [TimeAttribute]. Remove this attribute.", assemblyWeaver.Errors);
        Assert.Contains("Method 'System.Void MyInterface::MyMethod()' is abstract but has a [TimeAttribute]. Remove this attribute.", assemblyWeaver.Errors);
    }

    [Test]
    public void ClassWithAttribute()
    {
        ClearMessage();
        var type = assemblyWeaver.Assembly.GetType("ClassWithAttribute");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();
        Assert.AreEqual(GetMethodInfoField().Name, "Method");
        Assert.AreEqual(GetMethodInfoField().DeclaringType, type);
    }
    [Test]
    public void CheckErrors()
    {
        Assert.Contains("Method 'System.Void AbstractClassWithAttributeOnMethod::Method()' is abstract but has a [TimeAttribute]. Remove this attribute.", assemblyWeaver.Errors);
        Assert.Contains("Method 'System.Void MyInterface::MyMethod()' is abstract but has a [TimeAttribute]. Remove this attribute.", assemblyWeaver.Errors);
    }

    [Test]
    public void ClassWithConstructor()
    {
        ClearMessage();
        var type = assemblyWeaver.Assembly.GetType("ClassWithConstructor");
        Activator.CreateInstance(type);
        Assert.AreEqual(GetMethodInfoField().Name, ".ctor");
        Assert.AreEqual(GetMethodInfoField().DeclaringType, type);
    }

    [Test]
    public void ClassWithMethod()
    {
        ClearMessage();
        var type = assemblyWeaver.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();
        Assert.AreEqual(GetMethodInfoField().Name, "Method");
        Assert.AreEqual(GetMethodInfoField().DeclaringType, type);

    }

    [Test]
    public void MethodWithReturn()
    {
        ClearMessage();
        var type = assemblyWeaver.Assembly.GetType("MiscMethods");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.MethodWithReturn();
        Assert.AreEqual(GetMethodInfoField().Name, "MethodWithReturn");
        Assert.AreEqual(GetMethodInfoField().DeclaringType, type);
    }
    [Test]
    public void MethodWithAsync()
    {
        ClearMessage();
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();
        Thread.Sleep(100);
        Assert.AreEqual(GetMethodInfoField().Name, "Method");
        Assert.AreEqual(GetMethodInfoField().DeclaringType, type);
    }
    [Test]
    public void MethodWithAsyncReturn()
    {
        ClearMessage();
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.MethodWithReturn();
        Thread.Sleep(100);
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
        Verifier.Verify(assemblyWeaver.Assembly.CodeBase.Remove(0, 8));
    }
#endif

}