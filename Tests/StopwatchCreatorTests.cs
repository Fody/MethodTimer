using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentDate;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class StopwatchCreatorTests : IDisposable
{
    Assembly assembly;

    public StopwatchCreatorTests()
    {
        var moduleDefinition = ModuleDefinition.CreateModule("Fake", ModuleKind.Dll);
        var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyResolver = new MockAssemblyResolver(),
                LogError = LogError
            };
        weavingTask.FindReferences();
        weavingTask.InjectStopwatchType();
        using (var fileStream = File.OpenWrite("temp.dll"))
        {
            moduleDefinition.Write(fileStream);
            fileStream.Flush();
        }

        assembly = Assembly.Load(File.ReadAllBytes("temp.dll"));
    }

    void LogError(string error)
    {
        throw new Exception(error);
    }

    [Test]
    public void TestStopwatch()
    {
        var type = assembly.GetType("MethodTimer.Stopwatch");
        var instance = Activator.CreateInstance(type);
        Thread.Sleep(1.Seconds());

        var methodInformations = type.UnderlyingSystemType.GetMethods();

        methodInformations.First(x => x.Name == "Stop")
            .Invoke(instance, null);

        var elapsed = (long) methodInformations.First(x => x.Name == "GetElapsedMilliseconds")
            .Invoke(instance, null);
        Assert.GreaterOrEqual(elapsed, 1000);
    }

    [Test]
    public void PeVerify()
    {
        Verifier.Verify("temp.dll");
    }

    public void Dispose()
    {
        if (File.Exists("temp.dll"))
        {
            File.Delete("temp.dll");
        }
    }
}