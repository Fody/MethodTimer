using System.Linq;
using Fody;
using Xunit;
using Xunit.Abstractions;

public class AssemblyWithInterceptorAndFormattingWithoutOverloadTests :
    XunitLoggingBase
{
    [Fact]
    public void RaisesErrorForMissingOverload()
    {
        var weavingTask = new ModuleWeaver();
        var testResult = weavingTask.ExecuteTestRun("AssemblyWithInterceptorAndFormattingWithoutOverload.dll");

        Assert.Equal("Feature with parameter formatting is being used, but no useable log method can be found. Either disable the feature usage or update the logger signature to 'public static void Log(MethodBase methodBase, long milliseconds, string message)' or 'public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)'", testResult.Errors.Single().Text);
    }

    public AssemblyWithInterceptorAndFormattingWithoutOverloadTests(ITestOutputHelper output) :
        base(output)
    {
    }
}