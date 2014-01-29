using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

[TestFixture]
public class WithInterceptorTests
{
    AssemblyWeaver assemblyWeaver;
    FieldInfo methodBaseField;
    FieldInfo methodBaseStartField;

    public WithInterceptorTests()
    {
        var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\DebugWithInterceptor\AssemblyToProcess.dll");
        assemblyWeaver = new AssemblyWeaver(assemblyPath);
        var methodTimeLogger = assemblyWeaver.Assembly.GetType("MethodTimeLogger");
        methodBaseField = methodTimeLogger.GetField("MethodBase");
        methodBaseStartField = methodTimeLogger.GetField("MethodBaseOnStart");
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

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);
        var methodBase = methodBases.First();
        Assert.AreEqual(methodBase.Name, "Method");
        Assert.AreEqual(methodBase.DeclaringType, type);

        var methodBasesStart = GetMethodOnStartInfoField();
        Assert.AreEqual(1, methodBasesStart.Count);
        var methodBaseStart = methodBasesStart.First();
        Assert.AreEqual(methodBaseStart.Name, "Method");
        Assert.AreEqual(methodBaseStart.DeclaringType, type);
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

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(2, methodBases.Count);

        Assert.AreEqual(methodBases[0].Name, ".cctor");
        Assert.AreEqual(methodBases[0].DeclaringType, type);
        Assert.AreEqual(methodBases[1].Name, ".ctor");
        Assert.AreEqual(methodBases[1].DeclaringType, type);

        var methodBasesStart = GetMethodOnStartInfoField();
        Assert.AreEqual(2, methodBasesStart.Count);

        Assert.AreEqual(methodBasesStart[0].Name, ".cctor");
        Assert.AreEqual(methodBasesStart[0].DeclaringType, type);
        Assert.AreEqual(methodBasesStart[1].Name, ".ctor");
        Assert.AreEqual(methodBasesStart[1].DeclaringType, type);

    }

    [Test]
    public void ClassWithMethod()
    {
        ClearMessage();
        var type = assemblyWeaver.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);
        var methodBase = methodBases.First();
        Assert.AreEqual(methodBase.Name, "Method");
        Assert.AreEqual(methodBase.DeclaringType, type);

        var methodBasesStart = GetMethodOnStartInfoField();
        Assert.AreEqual(1, methodBasesStart.Count);
        var methodBaseStart = methodBasesStart.First();
        Assert.AreEqual(methodBaseStart.Name, "Method");
        Assert.AreEqual(methodBaseStart.DeclaringType, type);


    }

    [Test]
    public void GenericClassWithMethod()
    {
        ClearMessage();
        var type = assemblyWeaver.Assembly.GetType("GenericClassWithMethod`1[[System.String, mscorlib]]");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Method();

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);
        var methodBase = methodBases.First();
        Assert.AreEqual(methodBase.Name, "Method");
        Assert.That(methodBase.DeclaringType.Name.StartsWith("GenericClassWithMethod`1"));

        var methodBasesStart = GetMethodOnStartInfoField();
        Assert.AreEqual(1, methodBasesStart.Count);
        var methodBaseStart = methodBasesStart.First();
        Assert.AreEqual(methodBaseStart.Name, "Method");
        Assert.That(methodBaseStart.DeclaringType.Name.StartsWith("GenericClassWithMethod`1"));

    }

    [Test]
    public void MethodWithReturn()
    {
        ClearMessage();
        var type = assemblyWeaver.Assembly.GetType("MiscMethods");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.MethodWithReturn();

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);
        var methodBase = methodBases.First();

        Assert.AreEqual(methodBase.Name, "MethodWithReturn");
        Assert.AreEqual(methodBase.DeclaringType, type);

        var methodBasesStart = GetMethodOnStartInfoField();
        Assert.AreEqual(1, methodBasesStart.Count);
        var methodBaseStart = methodBasesStart.First();

        Assert.AreEqual(methodBaseStart.Name, "MethodWithReturn");
        Assert.AreEqual(methodBaseStart.DeclaringType, type);

    }

    [Test]
    public void MethodWithReturnAndCatchReThrow()
    {
        ClearMessage();
        var type = assemblyWeaver.Assembly.GetType("MiscMethods");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.MethodWithReturnAndCatchReThrow();

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);
        var methodBase = methodBases.First();

        Assert.AreEqual(methodBase.Name, "MethodWithReturnAndCatchReThrow");
        Assert.AreEqual(methodBase.DeclaringType, type);

        var methodBasesStart = GetMethodOnStartInfoField();
        Assert.AreEqual(1, methodBasesStart.Count);
        var methodBaseStart = methodBasesStart.First();

        Assert.AreEqual(methodBaseStart.Name, "MethodWithReturnAndCatchReThrow");
        Assert.AreEqual(methodBaseStart.DeclaringType, type);

    }

    //[Test]
    //public void MethodWithAsync()
    //{
    //    ClearMessage();
    //    var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
    //    var instance = (dynamic) Activator.CreateInstance(type);
    //    instance.Method();
    //    Thread.Sleep(100);
    //    var methodBases = GetMethodInfoField();
    //    Assert.AreEqual(1, methodBases.Count);
    //    var methodBase = methodBases.First();

    //    Assert.AreEqual(methodBase.Name, "Method");
    //    Assert.AreEqual(methodBase.DeclaringType, type);
    //}

    //[Test]
    //public void MethodWithAwait()
    //{
    //    ClearMessage();
    //    var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
    //    var instance = (dynamic) Activator.CreateInstance(type);
    //    instance.MethodWithAwait();
    //    Thread.Sleep(4000);
    //    var methodBases = GetMethodInfoField();
    //    Assert.AreEqual(1, methodBases.Count);
    //    var methodBase = methodBases.First();

    //    Assert.AreEqual(methodBase.Name, "MethodWithAwait");
    //    Assert.AreEqual(methodBase.DeclaringType, type);
    //}

    //[Test]
    //public void MethodWithAsyncReturn()
    //{
    //    ClearMessage();
    //    var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
    //    var instance = (dynamic) Activator.CreateInstance(type);
    //    instance.MethodWithReturn();
    //    Thread.Sleep(100);
    //    var methodBases = GetMethodInfoField();
    //    Assert.AreEqual(1, methodBases.Count);
    //    var methodBase = methodBases.First();
    //    Assert.AreEqual(methodBase.Name, "MethodWithReturn");
    //    Assert.AreEqual(methodBase.DeclaringType, type);
    //}

    void ClearMessage()
    {
        methodBaseField.SetValue(null,new List<MethodBase>());
        methodBaseStartField.SetValue(null, new List<MethodBase>());
    }

    List<MethodBase> GetMethodInfoField()
    {
        return (List<MethodBase>)methodBaseField.GetValue(null);
    }

    List<MethodBase> GetMethodOnStartInfoField()
    {
        return (List<MethodBase>)methodBaseStartField.GetValue(null);
    }


    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assemblyWeaver.Assembly.CodeBase.Remove(0, 8));
    }

}