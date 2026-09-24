using System.Linq;
using Fody;

// tests share static state and capture Trace output
[NotInParallel]
public class AssemblyWithInterceptorAndFormattingWithThisInStaticMemberTests
{
    [Test]
    public async Task RaisesErrorForThisInStaticMember()
    {
        var weavingTask = new ModuleWeaver();
        var testResult = weavingTask.ExecuteTestRun("AssemblyWithInterceptorAndFormattingWithThisInStaticMember.dll");
        await Assert.That(testResult.Errors.Single().Text).IsEqualTo("Could not process 'System.Void ClassWithMethod::Method(System.String,System.Int32)' because the format uses 'this' in a static context.");
    }
}