using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class WithInterceptorTests
{
    AssemblyWeaver assemblyWeaver;
    FieldInfo methodBaseField;

    public WithInterceptorTests()
    {
        var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\DebugWithInterceptor\AssemblyWithInterceptor.dll");
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

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);
        var methodBase = methodBases.First();
        Assert.AreEqual(methodBase.Name, "Method");
        Assert.AreEqual(methodBase.DeclaringType, type);
    }

    [Test]
    public void CheckErrors()
    {
        Assert.Contains("Method 'System.Void AbstractClassWithAttributeOnMethod::Method()' is abstract but has a [TimeAttribute]. Remove this attribute.", assemblyWeaver.Errors);
        Assert.Contains("Method 'System.Void MyInterface::MyMethod()' is abstract but has a [TimeAttribute]. Remove this attribute.", assemblyWeaver.Errors);
    }

    [Test]
    public async void ClassWithAsyncMethod()
    {
        await DebugRunner.CaptureDebugAsync(ClassWithAsyncMethodInvocation);

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);
        var methodBase = methodBases.First();

        Assert.AreEqual(methodBase.Name, "MethodWithAwait");
        //Assert.AreEqual(methodBase.DeclaringType, type);
    }

    async Task ClassWithAsyncMethodInvocation()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        await instance.MethodWithAwait();
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
    }
}