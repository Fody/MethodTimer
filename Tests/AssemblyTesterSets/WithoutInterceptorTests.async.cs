using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public partial class WithoutInterceptorTests
{
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

    [Fact]
    public void ClassWithGenericTaskWithoutMessageAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithGenericResultAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.DoSomethingWithoutMessageAsync<int>();
            task.Wait();
        });

        Assert.Single(message);
        Assert.StartsWith("ClassWithGenericResultAsyncMethod.DoSomethingWithoutMessageAsync", message.First());
    }

    [Fact]
    public void ClassWithGenericTaskWithMessageAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithGenericResultAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.DoSomethingWithMessageAsync<int>();
            task.Wait();
        });

        Assert.Single(message);
        Assert.StartsWith("ClassWithGenericResultAsyncMethod.DoSomethingWithMessageAsync", message.First());
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
}