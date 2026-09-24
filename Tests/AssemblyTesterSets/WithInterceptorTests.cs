using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fody;

// tests share static state and capture Trace output
[NotInParallel]
public class WithInterceptorTests
{
    static FieldInfo methodBaseField;
    static FieldInfo messagesField;
    static TestResult testResult;

    static WithInterceptorTests()
    {
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun("AssemblyWithInterceptor.dll",
            ignoreCodes: IgnoreCodes.GetIgnoreCoders()
#if NETCOREAPP2_1
            , runPeVerify: false
#endif
        );

        var methodTimeLogger = testResult.Assembly.GetType("MethodTimeLogger");
        methodBaseField = methodTimeLogger.GetField("MethodBase");
        messagesField = methodTimeLogger.GetField("Messages");
    }

    [Test]
    public async Task ClassWithExpressionBodiedMember()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithExpressionBodiedMember");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();
        var methodBases = GetMethodInfoField();
        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("get_FooBar");
        await Assert.That(type).IsEqualTo(methodBase.DeclaringType);
    }

    [Test]
    public async Task ClassWithMethod()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();
        var methodBases = GetMethodInfoField();
        await Assert.That(methodBases).HasSingleItem();
        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("Method");
        await Assert.That(type).IsEqualTo(methodBase.DeclaringType);
    }

    static void ClearMessage()
    {
        methodBaseField.SetValue(null, new List<MethodBase>());
        messagesField.SetValue(null, new List<string>());
    }

    static List<MethodBase> GetMethodInfoField() =>
        (List<MethodBase>) methodBaseField.GetValue(null);

    List<string> GetMessagesField() =>
        (List<string>)messagesField.GetValue(null);

    [Test]
    public async Task GenericClassWithMethod()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("GenericClassWithMethod`1[[System.String, mscorlib]]");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();

        var methodBases = GetMethodInfoField();
        await Assert.That(methodBases).HasSingleItem();
        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("Method");
        await Assert.That(methodBase.DeclaringType.Name).StartsWith("GenericClassWithMethod`1");
    }

    [Test]
    [Skip("todo")]
    public async Task ClassWithAsyncMethod()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task) instance.MethodWithAwaitAsync();
            task.Wait();
        });

        var methodBases = GetMethodInfoField();
        await Assert.That(methodBases).HasSingleItem();
        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("MethodWithAwaitAsync");
    }

    [Test]
    public async Task ClassWithGenericAsyncMethod()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task<int>)instance.GenericMethodAsync<int>();
            task.Wait();
        });

        var methodBases = GetMethodInfoField();
        await Assert.That(methodBases).HasSingleItem();
        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("GenericMethodAsync");
    }

    [Test]
    public async Task ClassWithAsyncMethodThatThrowsException()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        await TraceRunner.CaptureAsync(async () =>
        {
            try
            {
                await (Task) instance.MethodWithAwaitAndExceptionAsync();
            }
            catch (Exception)
            {
                // Expected
            }
        });

        var methodBases = GetMethodInfoField();

        // Make sure there are no 2, see https://github.com/Fody/MethodTimer/issues/124
        var allMethodBases = (from x in methodBases
                              where x.Name.Equals("MethodWithAwaitAndExceptionAsync")
                              select x).ToList();

        await Assert.That(allMethodBases).HasSingleItem();
    }

    [Test]
    public async Task ClassWithAsyncMethodWithExceptionAsync()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        await TraceRunner.CaptureAsync(async () =>
        {
            try
            {
                await (Task)instance.MethodWithAwaitAndExceptionAsync();
            }
            catch (Exception)
            {
                // Expected
            }
        });

        var methodBases = GetMethodInfoField();

        // Make sure there are no 2, see https://github.com/Fody/MethodTimer/issues/124
        var allMethodBases = (from x in methodBases
                              where x.Name.Equals("MethodWithAwaitAndExceptionAsync")
                              select x).ToList();

        await Assert.That(allMethodBases).HasSingleItem();
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ClassWithAsyncMethodWithFastPath(bool recurse)
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task) instance.MethodWithFastPathAsync(recurse);
            task.Wait();
        });

        var methodBases = GetMethodInfoField();

        // Interceptor can't deal with 2 test cases
        //await Assert.That(methodBases.Count).IsEqualTo(recurse ? 2 : 1);

        var methodBase = methodBases.Last();
        await Assert.That(methodBase.Name).IsEqualTo("MethodWithFastPathAsync");
    }

    [Test]
    public async Task LocalInstanceMethod()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithLocalMethods");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.MethodWithLocalInstanceMethod();
        var methodBases = GetMethodInfoField();
        await Assert.That(methodBases).HasSingleItem();
        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).Contains("LocalInstanceMethodToTime");
        await Assert.That(type).IsEqualTo(methodBase.DeclaringType);
    }

    [Test]
    public async Task LocalStaticMethod()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithLocalMethods");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.MethodWithLocalStaticMethod();
        var methodBases = GetMethodInfoField();
        await Assert.That(methodBases).HasSingleItem();
        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).Contains("LocalStaticMethodToTime");
        await Assert.That(type).IsEqualTo(methodBase.DeclaringType);
    }
}