﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fody;
using Xunit;

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

    static void ClearMessage()
    {
        methodBaseField.SetValue(null, new List<MethodBase>());
        messagesField.SetValue(null, new List<string>());
    }

    static List<MethodBase> GetMethodInfoField() =>
        (List<MethodBase>) methodBaseField.GetValue(null);

    List<string> GetMessagesField() =>
        (List<string>)messagesField.GetValue(null);

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
        ClearMessage();
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
    public void ClassWithGenericAsyncMethod()
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
        Assert.Single(methodBases);
        var methodBase = methodBases.First();
        Assert.Equal("GenericMethodAsync", methodBase.Name);
    }

    [Fact]
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

        Assert.Single(allMethodBases);
    }

    [Fact]
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

        Assert.Single(allMethodBases);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ClassWithAsyncMethodWithFastPath(bool recurse)
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
        //Assert.Equal(recurse ? 2 : 1, methodBases.Count);

        var methodBase = methodBases.Last();
        Assert.Equal("MethodWithFastPathAsync", methodBase.Name);
    }

    [Fact]
    public void LocalInstanceMethod()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithLocalMethods");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.MethodWithLocalInstanceMethod();
        var methodBases = GetMethodInfoField();
        Assert.Single(methodBases);
        var methodBase = methodBases.First();
        Assert.Contains("LocalInstanceMethodToTime", methodBase.Name);
        Assert.Equal(methodBase.DeclaringType, type);
    }

    [Fact]
    public void LocalStaticMethod()
    {
        ClearMessage();
        var type = testResult.Assembly.GetType("ClassWithLocalMethods");
        var instance = (dynamic) Activator.CreateInstance(type);
        instance.MethodWithLocalStaticMethod();
        var methodBases = GetMethodInfoField();
        Assert.Single(methodBases);
        var methodBase = methodBases.First();
        Assert.Contains("LocalStaticMethodToTime", methodBase.Name);
        Assert.Equal(methodBase.DeclaringType, type);
    }
}