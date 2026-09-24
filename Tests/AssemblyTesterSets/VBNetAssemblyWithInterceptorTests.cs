using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fody;

// tests share static state and capture Trace output
[NotInParallel]
public class VBNetAssemblyWithInterceptorTests
{
    static string assembly = "VBNetAssemblyWithInterceptor";
    static FieldInfo methodBaseField;
    static FieldInfo messagesField;
    static TestResult testResult;

    static VBNetAssemblyWithInterceptorTests()
    {
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun($"{assembly}.dll",
            ignoreCodes: IgnoreCodes.GetIgnoreCoders()
#if NETCOREAPP2_1
            , runPeVerify: false
#endif
        );

        var methodTimeLogger = testResult.Assembly.GetType($"{assembly}.MethodTimeLogger");
        methodBaseField = methodTimeLogger.GetField("MethodBase");
        messagesField = methodTimeLogger.GetField("Messages");
    }

    static void ClearMessage()
    {
        methodBaseField.SetValue(null, new List<MethodBase>());
        messagesField.SetValue(null, new List<string>());
    }

    static List<MethodBase> GetMethodInfoField() =>
        (List<MethodBase>)methodBaseField.GetValue(null);

    List<string> GetMessagesField() =>
        (List<string>)messagesField.GetValue(null);

    [Test]
    public async Task ClassWithAsyncMethod()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType($"{assembly}.ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithAwaitAsync();
            task.Wait();
        });

        var methodBases = GetMethodInfoField();
        await Assert.That(methodBases).HasSingleItem();
        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("MethodWithAwaitAsync");
    }
}