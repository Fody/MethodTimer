using System;
using Fody;
using Xunit;
#pragma warning disable 618

public class WithNopInterceptorTests
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
}