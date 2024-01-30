using System.Linq;
using Fody;
using Xunit;

public class AssemblyWithInterceptorAndFormattingWithThisInStaticMemberTests
{
    [Fact]
    public void RaisesErrorForThisInStaticMember()
    {
        var weaver = new ModuleWeaver();
        var testResult = weaver.ExecuteTestRun("AssemblyWithInterceptorAndFormattingWithThisInStaticMember.dll");
        Assert.Equal("Could not process 'System.Void ClassWithMethod::Method(System.String,System.Int32)' because the format uses 'this' in a static context.", testResult.Errors.Single().Text);
    }
}