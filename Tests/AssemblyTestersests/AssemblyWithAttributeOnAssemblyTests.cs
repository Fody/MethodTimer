using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class AssemblyWithAttributeOnAssemblyTests
{
	AssemblyWeaver assemblyWeaver;

	public AssemblyWithAttributeOnAssemblyTests()
	{
        var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyWithAttributeOnAssembly\bin\Debug\AssemblyWithAttributeOnAssembly.dll");
		assemblyWeaver = new AssemblyWeaver(assemblyPath);
	}

    [Test]
    public void ClassWithNoAttribute()
    {
        var message = DebugRunner.CaptureDebug(() =>
        {
            var type = assemblyWeaver.Assembly.GetType("ClassWithNoAttribute");
            var instance = (dynamic)Activator.CreateInstance(type);
            instance.Method();
        });
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithNoAttribute.Method "));
    }

    [Test]
    public void ClassWithAsyncMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithCompilerGeneratedTypes");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() =>
        {
            var task = (Task)instance.AsyncMethod();
            task.Wait();
        });

        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithCompilerGeneratedTypes.AsyncMethod "));
    }

    [Test]
    public void ClassWithYieldMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithCompilerGeneratedTypes");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() =>
        {
            var task = (IEnumerable<string>)instance.YieldMethod();
            task.ToList();
        });

        Assert.AreEqual(0, message.Count);
        //TODO: support yield
        //Assert.IsTrue(message.First().StartsWith("ClassWithCompilerGeneratedTypes.YieldMethod "));
    }

	[Test]
	public void PeVerify()
	{
		Verifier.Verify(assemblyWeaver.Assembly.CodeBase.Remove(0, 8));
	}

}