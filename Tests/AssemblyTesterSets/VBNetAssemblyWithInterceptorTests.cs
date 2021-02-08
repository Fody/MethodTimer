using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fody;
using Xunit;

public class VBNetAssemblyWithInterceptorTests
{
    static readonly string assembly = "VBNetAssemblyWithInterceptor";
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

    void ClearMessage()
    {
        methodBaseField.SetValue(null, new List<MethodBase>());
        messagesField.SetValue(null, new List<string>());
    }

    List<MethodBase> GetMethodInfoField()
    {
        return (List<MethodBase>)methodBaseField.GetValue(null);
    }

    List<string> GetMessagesField()
    {
        return (List<string>)messagesField.GetValue(null);
    }

    [Fact]
    public void ClassWithAsyncMethod()
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
        Assert.Single(methodBases);
        var methodBase = methodBases.First();
        Assert.Equal("MethodWithAwaitAsync", methodBase.Name);
    }

}
