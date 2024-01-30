using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Xunit;

public partial class WithoutInterceptorTests
{
    static TestResult testResult;

    static WithoutInterceptorTests()
    {
        var weaver = new ModuleWeaver();
        testResult = weaver.ExecuteTestRun("AssemblyWithoutInterceptor.dll",
            ignoreCodes: IgnoreCodes.GetIgnoreCoders()
#if NETCOREAPP2_1
            , runPeVerify: false
#endif
        );
    }

    [Fact]
    public void AssertAttributeIsRemoved()
    {
        var type = testResult.Assembly.GetType("TimeAttribute");
        Assert.Null(type);
    }

    [Fact]
    public void CheckErrors()
    {
        var errors = testResult.Errors
            .Select(x=>x.Text)
            .ToList();
        Assert.Contains("Method 'System.Void AbstractClassWithAttributeOnMethod::Method()' is abstract but has a [TimeAttribute]. Remove this attribute.", errors);
        Assert.Contains("Method 'System.Void MyInterface::MyMethod()' is abstract but has a [TimeAttribute]. Remove this attribute.", errors);
    }

    [Fact]
    public void ClassWithYieldMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithYieldMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (IEnumerable<string>)instance.YieldMethod();
            task.ToList();
        });

        Assert.Empty(message);
        //TODO: support yield
        //Assert.True(message.First().StartsWith("ClassWithYieldMethod.YieldMethod "));
    }

    [Fact]
    public void ClassWithConstructor()
    {
        var type = testResult.Assembly.GetType("ClassWithConstructor");
        var message = TraceRunner.Capture(() => Activator.CreateInstance(type));
        Assert.Equal(2, message.Count);
        Assert.StartsWith("ClassWithConstructor.cctor ", message[0]);
        Assert.StartsWith("ClassWithConstructor.ctor ", message[1]);
    }

    [Fact]
    public void ClassWithExpressionBodiedMember()
    {
        var type = testResult.Assembly.GetType("ClassWithExpressionBodiedMember");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() => instance.Method());
        Assert.StartsWith("ClassWithExpressionBodiedMember.get_FooBar ", message.First());
    }

    [Fact]
    public void ClassWithAttribute()
    {
        var type = testResult.Assembly.GetType("ClassWithAttribute");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() => instance.Method());
        Assert.Single(message);
        Assert.StartsWith("ClassWithAttribute.Method ", message.First());
    }

    [Fact]
    public void MethodWithReturnAndCatchReThrow()
    {
        var type = testResult.Assembly.GetType("MiscMethods");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() => instance.MethodWithReturnAndCatchReThrow());
        Assert.Single(message);
        Assert.StartsWith("MiscMethods.MethodWithReturnAndCatchReThrow ", message.First());
    }

    [Fact]
    public void ClassWithMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() => instance.Method());
        Assert.Single(message);
        Assert.StartsWith("ClassWithMethod.Method ", message.First());
    }

    [Fact]
    public void GenericClassWithMethod()
    {
        var type = testResult.Assembly.GetType("GenericClassWithMethod`1[[System.String, mscorlib]]");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() => instance.Method());
        Assert.Single(message);
        Assert.StartsWith("GenericClassWithMethod`1.Method ", message.First());
    }

    [Fact]
    public void MethodWithReturn()
    {
        var type = testResult.Assembly.GetType("MiscMethods");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() => instance.MethodWithReturn());
        Assert.Single(message);
        Assert.StartsWith("MiscMethods.MethodWithReturn ", message.First());
    }

    [Fact]
    public void InheritedClass()
    {
        var type = testResult.Assembly.GetType("InheritedClass");
        var message = TraceRunner.Capture(() => Activator.CreateInstance(type));
        Assert.Single(message);
        var first = message.First();
        Assert.True(first.StartsWith("InheritedClass.ctor "), first);
    }

    [Fact]
    public void InheritedClassDoingPropertyCall()
    {
        var type = testResult.Assembly.GetType("InheritedClassDoingPropertyCall");
        var message = TraceRunner.Capture(() => Activator.CreateInstance(type));
        Assert.Single(message);
        Assert.True(message[0].StartsWith("InheritedClassDoingPropertyCall.ctor "), message[0]);
    }

    [Fact]
    public void InheritedClassDoingConstructionCall()
    {
        var type = testResult.Assembly.GetType("InheritedClassDoingConstructionCall");
        var message = TraceRunner.Capture(() => Activator.CreateInstance(type));
        Assert.Single(message);
        Assert.True(message[0].StartsWith("InheritedClassDoingConstructionCall.ctor "), message[0]);
    }
}