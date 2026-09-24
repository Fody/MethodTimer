using System.Linq;
using Fody;

// tests share static state and capture Trace output
[NotInParallel]
public class AssemblyWithInterceptorAndFormattingWithWrongParametersTests
{
    [Test]
    public async Task RaisesErrorForWrongParameterNames()
    {
        var weavingTask = new ModuleWeaver();
        var testResult = weavingTask.ExecuteTestRun("AssemblyWithInterceptorAndFormattingWithWrongParameters.dll");
        await Assert.That(testResult.Errors.Single().Text).IsEqualTo("Could not process 'System.Void ClassWithMethod::Method(System.String,System.Int32)' because the format uses 'nonExistingParameterName' which is not available as method parameter.");
    }
}