using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

[TestFixture]
public class AssemblyWithAttributeOnModuleTests
{
    AssemblyWeaver assemblyWeaver;

    public AssemblyWithAttributeOnModuleTests()
    {
		var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\DebugWithAttributeOnModule\AssemblyToProcess.dll");
        assemblyWeaver = new AssemblyWeaver(assemblyPath);
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
    public void PeVerify()
    {
        Verifier.Verify(assemblyWeaver.Assembly.CodeBase.Remove(0, 8));
    }

}