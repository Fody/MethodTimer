using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fody;

// tests share static state and capture Trace output
[NotInParallel]
public class WithTimeSpanInterceptorAndFormattingTests
{
    static FieldInfo methodBaseField;
    static FieldInfo messagesField;
    static FieldInfo interceptorTypesField;
    static TestResult testResult;

    static WithTimeSpanInterceptorAndFormattingTests()
    {
        var weavingTask = new ModuleWeaver();

        testResult = weavingTask.ExecuteTestRun("AssemblyWithTimeSpanInterceptorAndFormatting.dll",
            ignoreCodes: IgnoreCodes.GetIgnoreCoders()
#if NETCOREAPP2_1
            , runPeVerify: false
#endif
        );

        var methodTimeLogger = testResult.Assembly.GetType("MethodTimeLogger");
        methodBaseField = methodTimeLogger.GetField("MethodBase");
        messagesField = methodTimeLogger.GetField("Messages");
        interceptorTypesField = methodTimeLogger.GetField("InterceptorTypes");
    }

    [Test]
    public async Task ClassWithMethod()
    {
        ClearMessage();

        var type = testResult.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Method("123", 42);

        var methodBases = GetMethodInfoField();
        await Assert.That(methodBases).HasSingleItem();

        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("Method");
        await Assert.That(type).IsEqualTo(methodBase.DeclaringType);

        var messages = GetMessagesField();
        await Assert.That(messages).HasSingleItem();

        var message = messages.First();
        await Assert.That(message).IsEqualTo("File name '123' with id '42'");

        // Note: must prefer TimeSpan above long
        var interceptorTypes = GetInterceptorTypesField();
        await Assert.That(interceptorTypes).HasSingleItem();

        var interceptorType = interceptorTypes.First();
        await Assert.That(interceptorType).IsEqualTo(InterceptorType.TimeSpan.ToString());
    }

    [Test]
    public async Task ClassWithMethodAndThis()
    {
        ClearMessage();

        var type = testResult.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.MethodWithThis("123", 42);

        var methodBases = GetMethodInfoField();
        await Assert.That(methodBases).HasSingleItem();

        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("MethodWithThis");
        await Assert.That(type).IsEqualTo(methodBase.DeclaringType);

        var messages = GetMessagesField();
        await Assert.That(messages).HasSingleItem();

        var message = messages.First();
        await Assert.That(message).IsEqualTo("Current object: 'TEST VALUE' | File name '123' with id '42'");

        // Note: must prefer TimeSpan above long
        var interceptorTypes = GetInterceptorTypesField();
        await Assert.That(interceptorTypes).HasSingleItem();

        var interceptorType = interceptorTypes.First();
        await Assert.That(interceptorType).IsEqualTo(InterceptorType.TimeSpan.ToString());
    }

    [Test]
    public async Task ClassWithMethodWithoutFormatting()
    {
        ClearMessage();

        var type = testResult.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.MethodWithoutFormatting("123", 42);

        var methodBases = GetMethodInfoField();
        await Assert.That(methodBases).HasSingleItem();

        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("MethodWithoutFormatting");
        await Assert.That(type).IsEqualTo(methodBase.DeclaringType);

        var messages = GetMessagesField();
        await Assert.That(messages).IsEmpty();

        // Note: must prefer TimeSpan above long
        var interceptorTypes = GetInterceptorTypesField();
        await Assert.That(interceptorTypes).HasSingleItem();

        var interceptorType = interceptorTypes.First();
        await Assert.That(interceptorType).IsEqualTo(InterceptorType.TimeSpan.ToString());
    }

    [Test]
    public async Task ClassWithAsyncMethod()
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
        await Assert.That(methodBases).HasSingleItem();

        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("MethodWithAwaitAsync");

        var messages = GetMessagesField();
        await Assert.That(messages).HasSingleItem();

        var message = messages.First();
        await Assert.That(message).IsEqualTo("File name '123' with id '42'");

        // Note: must prefer TimeSpan above long
        var interceptorTypes = GetInterceptorTypesField();
        await Assert.That(interceptorTypes).HasSingleItem();

        var interceptorType = interceptorTypes.First();
        await Assert.That(interceptorType).IsEqualTo(InterceptorType.TimeSpan.ToString());
    }

    [Test]
    public async Task ClassWithAsyncAndThisMethod()
    {
        ClearMessage();

        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithAwaitAndThisAsync("123", 42);
            task.Wait();
        });

        var methodBases = GetMethodInfoField();
        await Assert.That(methodBases).HasSingleItem();

        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("MethodWithAwaitAndThisAsync");

        var messages = GetMessagesField();
        await Assert.That(messages).HasSingleItem();

        var message = messages.First();
        await Assert.That(message).IsEqualTo("Current object: 'TEST VALUE' | File name '123' with id '42'");

        // Note: must prefer TimeSpan above long
        var interceptorTypes = GetInterceptorTypesField();
        await Assert.That(interceptorTypes).HasSingleItem();

        var interceptorType = interceptorTypes.First();
        await Assert.That(interceptorType).IsEqualTo(InterceptorType.TimeSpan.ToString());
    }

    [Test]
    public async Task ClassWithAsyncWithoutFormattingMethod()
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
        await Assert.That(methodBases).HasSingleItem();

        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("MethodWithAwaitWithoutFormattingAsync");

        var messages = GetMessagesField();
        await Assert.That(messages).IsEmpty();

        // Note: must prefer TimeSpan above long
        var interceptorTypes = GetInterceptorTypesField();
        await Assert.That(interceptorTypes).HasSingleItem();

        var interceptorType = interceptorTypes.First();
        await Assert.That(interceptorType).IsEqualTo(InterceptorType.TimeSpan.ToString());
    }

    // Note: in DEBUG because this only needs to run against optimized libraries
#if !DEBUG
    [Test]
    public async Task ClassWithAsyncMethodWithUnusedParameters()
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
        await Assert.That(error.Text).IsEqualTo("Parameter 'fileName' is not available on the async state machine. Probably it has been optimized away by the compiler. Please update the format so it excludes this parameter.");

        // Note: must prefer TimeSpan above long
        var interceptorTypes = GetInterceptorTypesField();
        await Assert.That(interceptorTypes).HasSingleItem();

        var interceptorType = interceptorTypes.First();
        await Assert.That(interceptorType).IsEqualTo(InterceptorType.TimeSpan.ToString());
    }
#endif

    [Test]
    public async Task ClassWithAsyncMethodThatThrowsException()
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
        await Assert.That(methodBase.Name).IsEqualTo("MethodWithAwaitAndExceptionAsync");

        var messages = GetMessagesField();
        await Assert.That(messages).HasSingleItem();

        var message = messages.First();
        await Assert.That(message).IsEqualTo("File name '123' with id '42'");

        // Note: must prefer TimeSpan above long
        var interceptorTypes = GetInterceptorTypesField();
        await Assert.That(interceptorTypes).HasSingleItem();

        var interceptorType = interceptorTypes.First();
        await Assert.That(interceptorType).IsEqualTo(InterceptorType.TimeSpan.ToString());
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ClassWithAsyncMethodWithFastPath(bool recurse)
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
        //await Assert.That(methodBases.Count).IsEqualTo(recurse ? 2 : 1);

        var methodBase = methodBases.Last();
        await Assert.That(methodBase.Name).IsEqualTo("MethodWithFastPathAsync");

        var messages = GetMessagesField();
        await Assert.That(messages.Count).IsEqualTo(recurse ? 2 : 1);

        var message = messages.First();
        await Assert.That(message).IsEqualTo("File name '123' with id '42'");

        // Note: must prefer TimeSpan above long
        var interceptorTypes = GetInterceptorTypesField();
        await Assert.That(interceptorTypes.Count).IsEqualTo(recurse ? 2 : 1);

        var interceptorType = interceptorTypes.First();
        await Assert.That(interceptorType).IsEqualTo(InterceptorType.TimeSpan.ToString());
    }

    static void ClearMessage()
    {
        methodBaseField.SetValue(null, new List<MethodBase>());
        messagesField.SetValue(null, new List<string>());
        interceptorTypesField.SetValue(null, new List<string>());
    }

    static List<MethodBase> GetMethodInfoField() =>
        (List<MethodBase>)methodBaseField.GetValue(null);

    static List<string> GetMessagesField() =>
        (List<string>)messagesField.GetValue(null);

    static List<string> GetInterceptorTypesField() =>
        (List<string>)interceptorTypesField.GetValue(null);
}