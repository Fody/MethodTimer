using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class WithoutInterceptorTests
{
    AssemblyWeaver assemblyWeaver;

    public WithoutInterceptorTests()
    {
        var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\DebugWithoutInterceptor\AssemblyWithoutInterceptor.dll");
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
    public async void ClassWithAsyncMethod()
    {
        var message = await DebugRunner.CaptureDebugAsync(ClassWithAsyncMethodInvocation);

        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.MethodWithAwait "));
    }

    private async Task ClassWithAsyncMethodInvocation()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        await instance.MethodWithAwait();
    }

    [Test]
    public async void ClassWithFastComplexAsyncMethod()
    {
        var message = await DebugRunner.CaptureDebugAsync(ClassWithFastComplexAsyncMethodInvocation);

        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwait "));
    }

    private async Task ClassWithFastComplexAsyncMethodInvocation()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        await instance.ComplexMethodWithAwait(0);
    }

    [Test]
    public async void ClassWithMediumComplexAsyncMethod()
    {
        var message = await DebugRunner.CaptureDebugAsync(ClassWithMediumComplexAsyncMethodInvocation);

        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwait "));
    }

    private async Task ClassWithMediumComplexAsyncMethodInvocation()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        await instance.ComplexMethodWithAwait(2);
    }

    [Test]
    public async void ClassWithSlowComplexAsyncMethod()
    {
        var message = await DebugRunner.CaptureDebugAsync(ClassWithSlowComplexAsyncMethodInvocation);

        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwait "));
    }

    private async Task ClassWithSlowComplexAsyncMethodInvocation()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        await instance.ComplexMethodWithAwait(100);
    }

    [Test]
    public void ClassWithConstructor()
    {
        var message = DebugRunner.CaptureDebug(() =>
            {
                var type = assemblyWeaver.Assembly.GetType("ClassWithConstructor");
                Activator.CreateInstance(type);
            });
        Assert.AreEqual(2, message.Count);
        Assert.IsTrue(message[0].StartsWith("ClassWithConstructor.cctor "));
        Assert.IsTrue(message[1].StartsWith("ClassWithConstructor.ctor "));
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
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAttribute.Method "));
    }

    [Test]
    public void MethodWithReturnAndCatchReThrow()
    {
        var message = DebugRunner.CaptureDebug(() =>
            {
                var type = assemblyWeaver.Assembly.GetType("MiscMethods");
                var instance = (dynamic) Activator.CreateInstance(type);
                instance.MethodWithReturnAndCatchReThrow();
            });
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("MiscMethods.MethodWithReturnAndCatchReThrow "));
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
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithMethod.Method "));
    }

    [Test]
    public void GenericClassWithMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("GenericClassWithMethod`1[[System.String, mscorlib]]");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() => instance.Method());
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("GenericClassWithMethod`1.Method "));

    }


    //[Test]
    //public void ClassWithAsyncMethod()
    //{
    //    var message = DebugRunner.CaptureDebug(() =>
    //        {
    //            var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
    //            var instance = (dynamic) Activator.CreateInstance(type);
    //            instance.Method();
    //        });
    //    Assert.AreEqual(1, message.Count);
    //    Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.Method "));

    //}
    //[Test]
    //public void MethodWithAwait()
    //{
    //    var message = DebugRunner.CaptureDebug(() =>
    //        {
    //            var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
    //            var instance = (dynamic) Activator.CreateInstance(type);
    //            instance.MethodWithAwait();
    //        });
    //    Assert.AreEqual(1, message.Count);
    //    var first = message.First();
    //    Assert.IsTrue(first.StartsWith("ClassWithAsyncMethod.MethodWithAwait "));

    //}

    [Test]
    public void MethodWithReturn()
    {
        var message = DebugRunner.CaptureDebug(() =>
            {
                var type = assemblyWeaver.Assembly.GetType("MiscMethods");
                var instance = (dynamic) Activator.CreateInstance(type);
                instance.MethodWithReturn();
            });
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("MiscMethods.MethodWithReturn "));

    }


    //[Test]
    //public void MethodWithAsyncReturn()
    //{
    //    var message = DebugRunner.CaptureDebug(() =>
    //        {
    //            var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
    //            var instance = (dynamic) Activator.CreateInstance(type);
    //            instance.MethodWithReturn();
    //        });
    //    Assert.AreEqual(1, message.Count);
    //    Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.MethodWithReturn "));

    //}

    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assemblyWeaver.Assembly.CodeBase.Remove(0, 8));
    }


}