using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fody;
using Xunit;
#pragma warning disable 618

public class WithoutInterceptorTests
{
    static TestResult testResult;

    static WithoutInterceptorTests()
    {
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun("AssemblyWithoutInterceptor.dll",
            ignoreCodes: IgnoreCodes.GetIgnoreCoders());
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
    public void MethodWithEmptyAsync()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithEmptyAsync();
            task.Wait();
        });

        Assert.Single(message);
        Assert.StartsWith("ClassWithAsyncMethod.MethodWithEmptyAsync ", message.First());
    }

    [Fact]
    public void ClassWithAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithAwaitAsync();
            task.Wait();
        });

        Assert.Single(message);
        Assert.StartsWith("ClassWithAsyncMethod.MethodWithAwaitAsync ", message.First());
    }

    [Fact]
    public void ClassWithAsyncMethodThatThrowsException()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            try
            {
                var task = (Task)instance.MethodWithAwaitAndExceptionAsync();
                task.Wait();
            }
            catch (Exception)
            {
                // Expected
            }
        });

        Assert.Single(message);
        Assert.StartsWith("ClassWithAsyncMethod.MethodWithAwaitAndExceptionAsync ", message.First());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ClassWithAsyncMethodWithFastPath(bool recurse)
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithFastPathAsync(recurse);
            task.Wait();
        });

        Assert.Equal(recurse ? 2 : 1, message.Count);
        Assert.StartsWith("ClassWithAsyncMethod.MethodWithFastPathAsync ", message.First());
    }

    [Fact]
    public void ClassWithExceptionAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.ComplexMethodWithAwaitAsync(-1);
            task.Wait();
        });

        Assert.Single(message);
        Assert.StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwaitAsync ", message.First());
    }

    [Fact]
    public void ClassWithFastComplexAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.ComplexMethodWithAwaitAsync(0);
            task.Wait();
        });

        Assert.Single(message);
        Assert.StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwaitAsync ", message.First());
    }

    [Fact]
    public void ClassWithMediumComplexAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.ComplexMethodWithAwaitAsync(2);
            task.Wait();
        });

        Assert.Single(message);
        Assert.StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwaitAsync ", message.First());
    }

    [Fact]
    public void ClassWithSlowComplexAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.ComplexMethodWithAwaitAsync(100);
            task.Wait();
        });

        Assert.Single(message);
        Assert.StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwaitAsync ", message.First());
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