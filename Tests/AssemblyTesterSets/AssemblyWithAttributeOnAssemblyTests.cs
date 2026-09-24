using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fody;

// tests share static state and capture Trace output
[NotInParallel]
public class AssemblyWithAttributeOnAssemblyTests
{
    static TestResult testResult;

    static AssemblyWithAttributeOnAssemblyTests()
    {
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun(
            assemblyPath: "AssemblyWithAttributeOnAssembly.dll",
            ignoreCodes: IgnoreCodes.GetIgnoreCoders()
#if NETCOREAPP2_1
            , runPeVerify: false
#endif
            );
    }

    [Test]
    public async Task ClassWithNoAttribute()
    {
        var message = TraceRunner.Capture(() =>
        {
            var instance = testResult.GetInstance("ClassWithNoAttribute");
            instance.Method();
        });
        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("ClassWithNoAttribute.Method ");
    }

    [Test]
    public async Task ClassWithAsyncMethod()
    {
        var instance = testResult.GetInstance("ClassWithCompilerGeneratedTypes");
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task) instance.AsyncMethod();
            task.Wait();
        });

        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("ClassWithCompilerGeneratedTypes.AsyncMethod ");
    }

    [Test]
    public async Task ClassWithYieldMethod()
    {
        var instance = testResult.GetInstance("ClassWithCompilerGeneratedTypes");
        var message = TraceRunner.Capture(() =>
        {
            var task = (IEnumerable<string>) instance.YieldMethod();
            task.ToList();
        });

        await Assert.That(message).IsEmpty();
        //TODO: support yield
        //await Assert.That(message.First().StartsWith("ClassWithCompilerGeneratedTypes.YieldMethod ")).IsTrue();
    }
}