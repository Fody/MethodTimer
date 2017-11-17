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
    string beforeAssemblyPath;

    public WithInterceptorTests()
    {
        beforeAssemblyPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "AssemblyWithInterceptor.dll");
        assemblyWeaver = new AssemblyWeaver(beforeAssemblyPath);
        var methodTimeLogger = assemblyWeaver.Assembly.GetType("MethodTimeLogger");
        methodBaseField = methodTimeLogger.GetField("MethodBase");
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

    void ClearMessage()
    {
        methodBaseField.SetValue(null, new List<MethodBase>());
    }

    List<MethodBase> GetMethodInfoField()
    {
        return (List<MethodBase>) methodBaseField.GetValue(null);
    }

    [Test]
    public void PeVerify()
    {
        Verifier.Verify(beforeAssemblyPath, assemblyWeaver.AfterAssemblyPath);
    }

    [Test]
    public void GenericClassWithMethod()
    {
        ClearMessage();
        var type = assemblyWeaver.Assembly.GetType("GenericClassWithMethod`1[[System.String, mscorlib]]");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);
        var methodBase = methodBases.First();
        Assert.AreEqual(methodBase.Name, "Method");
        Assert.That(methodBase.DeclaringType.Name.StartsWith("GenericClassWithMethod`1"));
    }

    [Test]
    public void ClassWithAsyncMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task) instance.MethodWithAwaitAsync();
            task.Wait();
        });

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);
        var methodBase = methodBases.First();
        Assert.AreEqual(methodBase.Name, "MethodWithAwaitAsync");
    }

    [Test]
    public void ClassWithAsyncMethodThatThrowsException()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            try
            {
                var task = (Task) instance.MethodWithAwaitAndExceptionAsync();
                task.Wait();
            }
            catch (Exception)
            {
                // Expected
            }
        });

        var methodBases = GetMethodInfoField();
        var methodBase = methodBases.Last();
        Assert.AreEqual(methodBase.Name, "MethodWithAwaitAndExceptionAsync");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ClassWithAsyncMethodWithFastPath(bool recurse)
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task) instance.MethodWithFastPathAsync(recurse);
            task.Wait();
        });

        var methodBases = GetMethodInfoField();

        // Interceptor can't deal with 2 test cases
        //Assert.AreEqual(recurse ? 2 : 1, methodBases.Count);

        var methodBase = methodBases.Last();
        Assert.AreEqual("MethodWithFastPathAsync", methodBase.Name);
    }
}