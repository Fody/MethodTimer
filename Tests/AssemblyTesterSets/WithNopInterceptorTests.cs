using System;
using Fody;
using Xunit;

public class WithNopInterceptorTests
{
    static TestResult testResult;

    static WithNopInterceptorTests()
    {
        var weaver = new ModuleWeaver();
        testResult = weaver.ExecuteTestRun("AssemblyWithNopInterceptor.dll");
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
}