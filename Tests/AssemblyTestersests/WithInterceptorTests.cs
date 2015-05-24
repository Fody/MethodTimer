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
        var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyWithInterceptor\bin\Debug\AssemblyWithInterceptor.dll");
        assemblyWeaver = new AssemblyWeaver(assemblyPath);
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
        Verifier.Verify(assemblyWeaver.Assembly.CodeBase.Remove(0, 8));
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
        DebugRunner.CaptureDebug(() =>
        {
            var task = (Task) instance.MethodWithAwait();
            task.Wait();
        });

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);
        var methodBase = methodBases.First();
        Assert.AreEqual(methodBase.Name, "MethodWithAwait");
    }
}