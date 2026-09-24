using System;
using Fody;

// tests share static state and capture Trace output
[NotInParallel]
public class WithNopInterceptorTests
{
    static TestResult testResult;

    static WithNopInterceptorTests()
    {
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun("AssemblyWithNopInterceptor.dll");
    }

    [Test]
    public async Task AssertAttributeIsRemoved()
    {
        var type = testResult.Assembly.GetType("TimeAttribute");
        await Assert.That(type).IsNull();
    }

    [Test]
    public void ClassWithMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Method();
    }
}