using System.Linq;
using Fody;
using Xunit;

public class AssemblyWithInterceptorAndFormattingWithWrongParametersTests
{
    [Fact]
    public void RaisesErrorForWrongParameterNames()
    {
        var weavingTask = new ModuleWeaver();
        var testResult = weavingTask.ExecuteTestRun("AssemblyWithInterceptorAndFormattingWithWrongParameters.dll");
        Assert.Equal("Could not process 'System.Void ClassWithMethod::Method(System.String,System.Int32)' because the format uses 'nonExistingParameterName' which is not available as method parameter.", testResult.Errors.Single().Text);
    }
}