using System;
using System.Collections.Generic;
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
        var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyWithoutInterceptor\bin\Debug\AssemblyWithoutInterceptor.dll");
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
    public void ClassWithYieldMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithYieldMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() =>
        {
            var task = (IEnumerable<string>)instance.YieldMethod();
            task.ToList();
        });

        Assert.AreEqual(0, message.Count);
        //TODO: support yield
        //Assert.IsTrue(message.First().StartsWith("ClassWithYieldMethod.YieldMethod "));
    }

    [Test]
    public void MethodWithEmptyAsync()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() =>
        {
            var task = (Task)instance.MethodWithEmptyAsync();
            task.Wait();
        });

        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.MethodWithEmptyAsync "));
    }

    [Test]
    public void ClassWithAsyncMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() =>
        {
            var task = (Task) instance.MethodWithAwaitAsync();
            task.Wait();
        });

        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.MethodWithAwaitAsync "));
    }

    [RequiresSTA]
    [TestCase(true)]
    [TestCase(false)]
    public void ClassWithAsyncMethodWithFastPath(bool recurse)
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() =>
        {
            var task = (Task)instance.MethodWithFastPathAsync(recurse);
            task.Wait();
        });

        Assert.AreEqual(recurse ? 2 : 1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.MethodWithFastPathAsync "));
    }

    [Test]
    public void ClassWithExceptionAsyncMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() =>
        {
            var task = (Task) instance.ComplexMethodWithAwaitAsync(-1);
            task.Wait();
        });

        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwaitAsync "));
    }

    [Test]
    public void ClassWithFastComplexAsyncMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() =>
        {
            var task = (Task) instance.ComplexMethodWithAwaitAsync(0);
            task.Wait();
        });

        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwaitAsync "));
    }

    [Test]
    public void ClassWithMediumComplexAsyncMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() =>
        {
            var task = (Task) instance.ComplexMethodWithAwaitAsync(2);
            task.Wait();
        });

        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwaitAsync "));
    }

    [Test]
    public void ClassWithSlowComplexAsyncMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAsyncMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() =>
        {
            var task = (Task) instance.ComplexMethodWithAwaitAsync(100);
            task.Wait();
        });

        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAsyncMethod.ComplexMethodWithAwaitAsync "));
    }

    [Test]
    public void ClassWithConstructor()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithConstructor");
        var message = DebugRunner.CaptureDebug(() => Activator.CreateInstance(type));
        Assert.AreEqual(2, message.Count);
        Assert.IsTrue(message[0].StartsWith("ClassWithConstructor.cctor "));
        Assert.IsTrue(message[1].StartsWith("ClassWithConstructor.ctor "));
    }

    [Test]
    public void ClassWithAttribute()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithAttribute");
        var instance = (dynamic) Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() => instance.Method());
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithAttribute.Method "));
    }

    [Test]
    public void MethodWithReturnAndCatchReThrow()
    {
        var type = assemblyWeaver.Assembly.GetType("MiscMethods");
        var instance = (dynamic) Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() => instance.MethodWithReturnAndCatchReThrow());
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("MiscMethods.MethodWithReturnAndCatchReThrow "));
    }

    [Test]
    public void ClassWithMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic) Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() => instance.Method());
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithMethod.Method "));
    }

    [Test]
    public void GenericClassWithMethod()
    {
        var type = assemblyWeaver.Assembly.GetType("GenericClassWithMethod`1[[System.String, mscorlib]]");
        var instance = (dynamic) Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() => instance.Method());
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("GenericClassWithMethod`1.Method "));

    }


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
        var type = assemblyWeaver.Assembly.GetType("MiscMethods");
        var instance = (dynamic) Activator.CreateInstance(type);
        var message = DebugRunner.CaptureDebug(() => instance.MethodWithReturn());
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message.First().StartsWith("MiscMethods.MethodWithReturn "));
    }

    [Test]
    public void InheritedClass()
    {
        var type = assemblyWeaver.Assembly.GetType("InheritedClass");
        var message = DebugRunner.CaptureDebug(() => Activator.CreateInstance(type));
        Assert.AreEqual(1, message.Count);
        var first = message.First();
        Assert.IsTrue(first.StartsWith("InheritedClass.ctor "), first);
    }

    [Test]
    public void InheritedClassDoingPropertyCall()
    {
        var type = assemblyWeaver.Assembly.GetType("InheritedClassDoingPropertyCall");
        var message = DebugRunner.CaptureDebug(() => Activator.CreateInstance(type));
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message[0].StartsWith("InheritedClassDoingPropertyCall.ctor "), message[0]);
    }
    [Test]
    public void InheritedClassDoingConstructionCall()
    {
        var type = assemblyWeaver.Assembly.GetType("InheritedClassDoingConstructionCall");
        var message = DebugRunner.CaptureDebug(() => Activator.CreateInstance(type));
        Assert.AreEqual(1, message.Count);
        Assert.IsTrue(message[0].StartsWith("InheritedClassDoingConstructionCall.ctor "), message[0]);
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