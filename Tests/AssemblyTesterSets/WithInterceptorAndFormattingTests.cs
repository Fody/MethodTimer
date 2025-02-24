using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fody;
using Xunit;

public class WithInterceptorAndFormattingTests
{
    static FieldInfo methodBaseField;
    static FieldInfo messagesField;
    static TestResult testResult;

    static WithInterceptorAndFormattingTests()
    {
        var weavingTask = new ModuleWeaver();

        testResult = weavingTask.ExecuteTestRun("AssemblyWithInterceptorAndFormatting.dll",
            ignoreCodes: IgnoreCodes.GetIgnoreCoders()
#if NETCOREAPP2_1
            , runPeVerify: false
#endif
        );

        var methodTimeLogger = testResult.Assembly.GetType("MethodTimeLogger");
        methodBaseField = methodTimeLogger.GetField("MethodBase");
        messagesField = methodTimeLogger.GetField("Messages");
    }

    [Fact]
    public void ClassWithMethod()
    {
        ClearMessage();

        var type = testResult.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Method("123", 42);

        var methodBases = GetMethodInfoField();
        Assert.Single(methodBases);

        var methodBase = methodBases.First();
        Assert.Equal("Method", methodBase.Name);
        Assert.Equal(methodBase.DeclaringType, type);

        var messages = GetMessagesField();
        Assert.Single(messages);

        var message = messages.First();
        Assert.Equal("File name '123' with id '42'", message);
    }

    [Fact]
    public void ClassWithMethodWithoutFormatting()
    {
        ClearMessage();

        var type = testResult.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.MethodWithoutFormatting("123", 42);

        var methodBases = GetMethodInfoField();
        Assert.Single(methodBases);

        var methodBase = methodBases.First();
        Assert.Equal("MethodWithoutFormatting", methodBase.Name);
        Assert.Equal(methodBase.DeclaringType, type);

        var messages = GetMessagesField();
        Assert.Empty(messages);
    }

    [Fact]
    public void ClassWithAsyncMethod()
    {
        ClearMessage();

        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithAwaitAsync("123", 42);
            task.Wait();
        });

        var methodBases = GetMethodInfoField();
        Assert.Single(methodBases);

        var methodBase = methodBases.First();
        Assert.Equal("MethodWithAwaitAsync", methodBase.Name);

        var messages = GetMessagesField();
        Assert.Single(messages);

        var message = messages.First();
        Assert.Equal("File name '123' with id '42'", message);
    }

    [Fact]
    public void ClassWithAsyncWithoutFormattingMethod()
    {
        ClearMessage();

        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithAwaitWithoutFormattingAsync("123", 42);
            task.Wait();
        });

        var methodBases = GetMethodInfoField();
        Assert.Single(methodBases);

        var methodBase = methodBases.First();
        Assert.Equal("MethodWithAwaitWithoutFormattingAsync", methodBase.Name);

        var messages = GetMessagesField();
        Assert.Empty(messages);
    }

    // Note: in DEBUG because this only needs to run against optimized libraries
#if !DEBUG
    [Fact]
    public void ClassWithAsyncMethodWithUnusedParameters()
    {
        ClearMessage();

        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithAwaitButUnusedParametersAsync("123", 42);
            task.Wait();
        });

        var error = testResult.Errors.First();
        Assert.Equal("Parameter 'fileName' is not available on the async state machine. Probably it has been optimized away by the compiler. Please update the format so it excludes this parameter.", error.Text);
    }
#endif

    [Fact]
    public void ClassWithAsyncMethodThatThrowsException()
    {
        ClearMessage();

        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            try
            {
                var task = (Task)instance.MethodWithAwaitAndExceptionAsync("123", 42);
                task.Wait();
            }
            catch (Exception)
            {
                // Expected
            }
        });

        var methodBases = GetMethodInfoField();
        var methodBase = methodBases.Last();
        Assert.Equal("MethodWithAwaitAndExceptionAsync", methodBase.Name);

        var messages = GetMessagesField();
        Assert.Single(messages);

        var message = messages.First();
        Assert.Equal("File name '123' with id '42'", message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ClassWithAsyncMethodWithFastPath(bool recurse)
    {
        ClearMessage();

        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithFastPathAsync(recurse, "123", 42);
            task.Wait();
        });

        var methodBases = GetMethodInfoField();

        // Interceptor can't deal with 2 test cases
        //Assert.Equal(recurse ? 2 : 1, methodBases.Count);

        var methodBase = methodBases.Last();
        Assert.Equal("MethodWithFastPathAsync", methodBase.Name);

        var messages = GetMessagesField();
        Assert.Equal(recurse ? 2 : 1, messages.Count);

        var message = messages.First();
        Assert.Equal("File name '123' with id '42'", message);
    }

    [Fact]
    public void ClassWithGenericAsyncMethod()
    {
        ClearMessage();

        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithGenericResultAsync<int>();
            task.Wait();
        });

        var methodBases = GetMethodInfoField();
        var methodBase = methodBases.Last();
        Assert.Equal("MethodWithGenericResultAsync", methodBase.Name);

        var messages = GetMessagesField();
        Assert.Single(messages);

        var message = messages.First();
        Assert.Equal("some message", message);
    }

    static void ClearMessage()
    {
        methodBaseField.SetValue(null, new List<MethodBase>());
        messagesField.SetValue(null, new List<string>());
    }

    static List<MethodBase> GetMethodInfoField() =>
        (List<MethodBase>)methodBaseField.GetValue(null);

    static List<string> GetMessagesField() =>
        (List<string>)messagesField.GetValue(null);
}