using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class AssemblyWithAttributeOnModuleTests
{
    AssemblyWeaver assemblyWeaver;
    string beforeAssemblyPath;

    public AssemblyWithAttributeOnModuleTests()
    {
        beforeAssemblyPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "AssemblyWithAttributeOnModule.dll");
        assemblyWeaver = new AssemblyWeaver(beforeAssemblyPath);
    }

    [Test]
    public void ClassWithNoAttribute()
    {
        var message = DebugRunner.CaptureDebug(() =>
        {
            var type = assemblyWeaver.Assembly.GetType("ClassWithNoAttribute");
            var instance = (dynamic) Activator.CreateInstance(type);
            instance.Method();
        });
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithNoAttribute.Method "));
    }

    [Test]
    public void ClassWithAsyncMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithCompilerGeneratedTypes");
        var instance = (dynamic) Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() =>
        {
            var task = (Task) instance.AsyncMethod();
            task.Wait();
        });

        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithCompilerGeneratedTypes.AsyncMethod "));
    }

    [Test]
    public void ClassWithYieldMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithCompilerGeneratedTypes");
        var instance = (dynamic) Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() =>
        {
            var task = (IEnumerable<string>) instance.YieldMethod();
            task.ToList();
        });

        Assert.AreEqual(0, message.Count);
        //TODO: support yield
        //Assert.IsTrue(message.First().StartsWith("ClassWithCompilerGeneratedTypes.YieldMethod "));
    }

    [Test]
    public void PeVerify()
    {
        Verifier.Verify(beforeAssemblyPath, assemblyWeaver.AfterAssemblyPath);
    }

}