using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class WithInterceptorAndFormattingTests
{
    AssemblyWeaver assemblyWeaver;
    FieldInfo methodBaseField;
    FieldInfo messagesField;
    string beforeAssemblyPath;

    public WithInterceptorAndFormattingTests()
    {
        beforeAssemblyPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "AssemblyWithInterceptorAndFormatting.dll");
        assemblyWeaver = new AssemblyWeaver(beforeAssemblyPath);
        var methodTimeLogger = assemblyWeaver.Assembly.GetType("MethodTimeLogger");
        methodBaseField = methodTimeLogger.GetField("MethodBase");
        messagesField = methodTimeLogger.GetField("Messages");
    }

    [Test]
    public void ClassWithMethod()
    {
        ClearMessage();

        var type = assemblyWeaver.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Method("123", 42);

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);

        var methodBase = methodBases.First();
        Assert.AreEqual(methodBase.Name, "Method");
        Assert.AreEqual(methodBase.DeclaringType, type);

        var messages = GetMessagesField();
        Assert.AreEqual(1, messages.Count);

        var message = messages.First();
        Assert.AreEqual(message, "File name '123' with id '42'");
    }

    [Test]
    public void ClassWithMethodWithoutFormatting()
    {
        ClearMessage();

        var type = assemblyWeaver.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.MethodWithoutFormatting("123", 42);

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);

        var methodBase = methodBases.First();
        Assert.AreEqual(methodBase.Name, "MethodWithoutFormatting");
        Assert.AreEqual(methodBase.DeclaringType, type);

        var messages = GetMessagesField();
        Assert.AreEqual(0, messages.Count);
    }

    [Apartment(ApartmentState.STA)]
    [Test]
    public void ClassWithAsyncMethod()
    {
        ClearMessage();

        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithAwaitAsync("123", 42);
            task.Wait();
        });

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);

        var methodBase = methodBases.First();
        Assert.AreEqual(methodBase.Name, "MethodWithAwaitAsync");

        var messages = GetMessagesField();
        Assert.AreEqual(1, messages.Count);

        var message = messages.First();
        Assert.AreEqual(message, "File name '123' with id '42'");
    }

    [Apartment(ApartmentState.STA)]
    [Test]
    public void ClassWithAsyncWithoutFormattingMethod()
    {
        ClearMessage();

        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithAwaitWithoutFormattingAsync("123", 42);
            task.Wait();
        });

        var methodBases = GetMethodInfoField();
        Assert.AreEqual(1, methodBases.Count);

        var methodBase = methodBases.First();
        Assert.AreEqual(methodBase.Name, "MethodWithAwaitWithoutFormattingAsync");

        var messages = GetMessagesField();
        Assert.AreEqual(0, messages.Count);
    }

    // Note: in DEBUG because this only needs to run against optimized libraries
#if !DEBUG
    [Apartment(ApartmentState.STA)]
    [Test]
    public void ClassWithAsyncMethodWithUnusedParameters()
    {
        ClearMessage();

        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithAwaitButUnusedParametersAsync("123", 42);
            task.Wait();
        });

        Assert.AreNotEqual(0, assemblyWeaver.Errors);

        var error = assemblyWeaver.Errors.First();
        Assert.AreEqual("Parameter 'fileName' is not available on the async state machine. Probably it has been optimized away by the compiler. Please update the format so it excludes this parameter.", error);
    }
#endif

    [Apartment(ApartmentState.STA)]
    [Test]
    public void ClassWithAsyncMethodThatThrowsException()
    {
        ClearMessage();

        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
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
        Assert.AreEqual(methodBase.Name, "MethodWithAwaitAndExceptionAsync");

        var messages = GetMessagesField();
        Assert.AreEqual(1, messages.Count);

        var message = messages.First();
        Assert.AreEqual(message, "File name '123' with id '42'");
    }

    [Apartment(ApartmentState.STA)]
    [TestCase(true)]
    [TestCase(false)]
    public void ClassWithAsyncMethodWithFastPath(bool recurse)
    {
        ClearMessage();

        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        TraceRunner.Capture(() =>
        {
            var task = (Task)instance.MethodWithFastPathAsync(recurse, "123", 42);
            task.Wait();
        });

        var methodBases = GetMethodInfoField();

        // Interceptor can't deal with 2 test cases
        //Assert.AreEqual(recurse ? 2 : 1, methodBases.Count);

        var methodBase = methodBases.Last();
        Assert.AreEqual("MethodWithFastPathAsync", methodBase.Name);

        var messages = GetMessagesField();
        Assert.AreEqual(recurse ? 2 : 1, messages.Count);

        var message = messages.First();
        Assert.AreEqual(message, "File name '123' with id '42'");
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

    [Test]
    public void PeVerify()
    {
        Verifier.Verify(beforeAssemblyPath, assemblyWeaver.AfterAssemblyPath);
    }
}