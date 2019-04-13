using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fody;
using Xunit;
using Xunit.Abstractions;

public class WithInterceptorTests :
    XunitLoggingBase
{
    static FieldInfo methodBaseField;
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
    }

    [Fact]
    public void ClassWithExpressionBodiedMember()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithExpressionBodiedMember");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();
        var methodBases = GetMethodInfoField();
        var methodBase = methodBases.First();
        Assert.Equal("get_FooBar", methodBase.Name);
        Assert.Equal(methodBase.DeclaringType, type);
    }

    [Fact]
    public void ClassWithMethod()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();
        var methodBases = GetMethodInfoField();
        Assert.Single(methodBases);
        var methodBase = methodBases.First();
        Assert.Equal("Method", methodBase.Name);
        Assert.Equal(methodBase.DeclaringType, type);
    }

    void ClearMessage()
    {
        methodBaseField.SetValue(null, new List<MethodBase>());
    }

    List<MethodBase> GetMethodInfoField()
    {
        return (List<MethodBase>) methodBaseField.GetValue(null);
    }

    [Fact]
    public void GenericClassWithMethod()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("GenericClassWithMethod`1[[System.String, mscorlib]]");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.Method();

        var methodBases = GetMethodInfoField();
        Assert.Single(methodBases);
        var methodBase = methodBases.First();
        Assert.Equal("Method", methodBase.Name);
        Assert.StartsWith("GenericClassWithMethod`1", methodBase.DeclaringType.Name);
    }

    [Fact(Skip = "todo")]
    public void ClassWithAsyncMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task) instance.MethodWithAwaitAsync();
            task.Wait();
        });

        var methodBases = GetMethodInfoField();
        Assert.Single(methodBases);
        var methodBase = methodBases.First();
        Assert.Equal("MethodWithAwaitAsync", methodBase.Name);
    }

    [Fact]
    public void ClassWithAsyncMethodThatThrowsException()
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            try
            {
                var task = (Task) instance.MethodWithAwaitAndExceptionAsync();
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
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ClassWithAsyncMethodWithFastPath(bool recurse)
    {
        var type = testResult.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task) instance.MethodWithFastPathAsync(recurse);
            task.Wait();
        });

        var methodBases = GetMethodInfoField();

        // Interceptor can't deal with 2 test cases
        //Assert.Equal(recurse ? 2 : 1, methodBases.Count);

        var methodBase = methodBases.Last();
        Assert.Equal("MethodWithFastPathAsync", methodBase.Name);
    }

    public WithInterceptorTests(ITestOutputHelper output) :
        base(output)
    {
    }
}