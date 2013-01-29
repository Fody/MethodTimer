using System;
using System.IO;
using NUnit.Framework;

[TestFixture]
public class WithoutInterceptorTests
{
    AssemblyWeaver assemblyWeaver;

    public WithoutInterceptorTests()
    {
        var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\DebugWithoutInterceptor\AssemblyToProcess.dll");
        assemblyWeaver = new AssemblyWeaver(assemblyPath);
    }

    [Test]
    public void AssertAttributeIsRemoved()
    {
        var type = assemblyWeaver.Assembly.GetType("TimeAttribute");
        Assert.IsNull(type);
    }

    [Test]
    public void CheckErrors()
    {
        Assert.Contains("Method 'System.Void AbstractClassWithAttributeOnMethod::Method()' is abstract but has a [TimeAttribute]. Remove this attribute.", assemblyWeaver.Errors);
        Assert.Contains("Method 'System.Void MyInterface::MyMethod()' is abstract but has a [TimeAttribute]. Remove this attribute.", assemblyWeaver.Errors);
    }

[Test]
    public void ClassWithConstructor()
    {
        var message = DebugRunner.CaptureDebug(() =>
            {
                var type = assemblyWeaver.Assembly.GetType("ClassWithConstructor");
                Activator.CreateInstance(type);
            });
        Assert.IsTrue(message.StartsWith("ClassWithConstructor.ctor "));
    }
    [Test]
    public void ClassWithAttribute()
    {
        var message = DebugRunner.CaptureDebug(() =>
            {
                var type = assemblyWeaver.Assembly.GetType("ClassWithAttribute");
                var instance = (dynamic) Activator.CreateInstance(type);
                instance.Method();
            });
        Assert.IsTrue(message.StartsWith("ClassWithAttribute.Method "));
    }

    [Test]
    public void ClassWithMethod()
    {
        var message = DebugRunner.CaptureDebug(() =>
            {
                var type = assemblyWeaver.Assembly.GetType("ClassWithMethod");
                var instance = (dynamic) Activator.CreateInstance(type);
                instance.Method();
            });
        Assert.IsTrue(message.StartsWith("ClassWithMethod.Method "));

    }
#if(DEBUG)
    [Test]
    public void ClassWithAsyncMethod()
    {
        var message = DebugRunner.CaptureDebug(() =>
            {
                var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
                var instance = (dynamic) Activator.CreateInstance(type);
                instance.Method();
            });
        Assert.IsTrue(message.StartsWith("ClassWithAsyncMethod.Method "));

    }
#endif

    [Test]
    public void MethodWithReturn()
    {
        var message = DebugRunner.CaptureDebug(() =>
            {
                var type = assemblyWeaver.Assembly.GetType("MiscMethods");
                var instance = (dynamic) Activator.CreateInstance(type);
                instance.MethodWithReturn();
            });
        Assert.IsTrue(message.StartsWith("MiscMethods.MethodWithReturn "));

    }
#if(DEBUG)
    [Test]
    public void MethodWithAsyncReturn()
    {
        var message = DebugRunner.CaptureDebug(() =>
            {
                var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
                var instance = (dynamic) Activator.CreateInstance(type);
                instance.MethodWithReturn();
            });
        Assert.IsTrue(message.StartsWith("ClassWithAsyncMethod.MethodWithReturn "));

    }
#endif



#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assemblyWeaver.Assembly.CodeBase.Remove(0, 8));
    }
#endif

}