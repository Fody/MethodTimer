using System;
using Fody;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class WithNopInterceptorTests :
    VerifyBase
{
    static TestResult testResult;

    static WithNopInterceptorTests()
    {
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun("AssemblyWithNopInterceptor.dll");
    }

    [Fact]
    public void AssertAttributeIsRemoved()
    {
        var type = testResult.Assembly.GetType("TimeAttribute");
        Assert.Null(type);
    }

    [Fact]
    public void ClassWithMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Method();
    }

    public WithNopInterceptorTests(ITestOutputHelper output) :
        base(output)
    {
    }
}