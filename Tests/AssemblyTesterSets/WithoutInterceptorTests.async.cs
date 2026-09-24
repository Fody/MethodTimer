using System;
using System.Linq;
using System.Threading.Tasks;

public partial class WithoutInterceptorTests
{
    [Test]
    public async Task MethodWithEmptyAsync()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithEmptyAsync();
            task.Wait();
        });

        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("ClassWithAsyncMethod.MethodWithEmptyAsync ");
    }

    [Test]
    public async Task ClassWithAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithAwaitAsync();
            task.Wait();
        });

        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("ClassWithAsyncMethod.MethodWithAwaitAsync ");
    }

    [Test]
    public async Task ClassWithAsyncMethodThatThrowsException()
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

        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("ClassWithAsyncMethod.MethodWithAwaitAndExceptionAsync ");
    }

    [Test]
    public async Task ClassWithGenericTaskWithoutMessageAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithGenericResultAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = await TraceRunner.CaptureAsync(async () =>
        {
            var task = (Task)instance.DoSomethingWithoutMessageAsync<int>();
            await task;
        });

        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("ClassWithGenericResultAsyncMethod.DoSomethingWithoutMessageAsync");
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ClassWithAsyncMethodWithFastPath(bool recurse)
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithFastPathAsync(recurse);
            task.Wait();
        });

        await Assert.That(message.Count).IsEqualTo(recurse ? 2 : 1);
        await Assert.That(message.First()).StartsWith("ClassWithAsyncMethod.MethodWithFastPathAsync ");
    }

    [Test]
    public async Task ClassWithExceptionAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.ComplexMethodWithAwaitAsync(-1);
            task.Wait();
        });

        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwaitAsync ");
    }

    [Test]
    public async Task ClassWithFastComplexAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.ComplexMethodWithAwaitAsync(0);
            task.Wait();
        });

        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwaitAsync ");
    }

    [Test]
    public async Task ClassWithMediumComplexAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.ComplexMethodWithAwaitAsync(2);
            task.Wait();
        });

        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwaitAsync ");
    }

    [Test]
    public async Task ClassWithSlowComplexAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task)instance.ComplexMethodWithAwaitAsync(100);
            task.Wait();
        });

        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwaitAsync ");
    }
}