using System.Linq;
using Fody;

// tests share static state and capture Trace output
[NotInParallel]
public class AssemblyWithInterceptorAndFormattingWithoutOverloadTests
{
    [Test]
    public async Task RaisesErrorForMissingOverload()
    {
        var weavingTask = new ModuleWeaver();
        var testResult = weavingTask.ExecuteTestRun("AssemblyWithInterceptorAndFormattingWithoutOverload.dll");

        await Assert.That(testResult.Errors.Single().Text).IsEqualTo("Feature with parameter formatting is being used, but no useable log method can be found. Either disable the feature usage or update the logger signature to 'public static void Log(MethodBase methodBase, long milliseconds, string message)' or 'public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)'");
    }
}