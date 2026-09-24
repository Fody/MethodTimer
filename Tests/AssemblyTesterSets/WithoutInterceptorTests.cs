using System;
using System.Collections.Generic;
using System.Linq;
using Fody;

// tests share static state and capture Trace output
[NotInParallel]
public partial class WithoutInterceptorTests
{
    static TestResult testResult;

    static WithoutInterceptorTests()
    {
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun("AssemblyWithoutInterceptor.dll",
            ignoreCodes: IgnoreCodes.GetIgnoreCoders()
#if NETCOREAPP2_1
            , runPeVerify: false
#endif
        );
    }

    [Test]
    public async Task AssertAttributeIsRemoved()
    {
        var type = testResult.Assembly.GetType("TimeAttribute");
        await Assert.That(type).IsNull();
    }

    [Test]
    public async Task CheckErrors()
    {
        var errors = testResult.Errors
            .Select(x=>x.Text)
            .ToList();
        await Assert.That(errors).Contains("Method 'System.Void AbstractClassWithAttributeOnMethod::Method()' is abstract but has a [TimeAttribute]. Remove this attribute.");
        await Assert.That(errors).Contains("Method 'System.Void MyInterface::MyMethod()' is abstract but has a [TimeAttribute]. Remove this attribute.");
    }

    [Test]
    public async Task ClassWithYieldMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithYieldMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() =>
        {
            var task = (IEnumerable<string>)instance.YieldMethod();
            task.ToList();
        });

        await Assert.That(message).IsEmpty();
        //TODO: support yield
        //await Assert.That(message.First().StartsWith("ClassWithYieldMethod.YieldMethod ")).IsTrue();
    }

    [Test]
    public async Task ClassWithConstructor()
    {
        var type = testResult.Assembly.GetType("ClassWithConstructor");
        var message = TraceRunner.Capture(() => Activator.CreateInstance(type));
        await Assert.That(message.Count).IsEqualTo(2);
        await Assert.That(message[0]).StartsWith("ClassWithConstructor.cctor ");
        await Assert.That(message[1]).StartsWith("ClassWithConstructor.ctor ");
    }

    [Test]
    public async Task ClassWithExpressionBodiedMember()
    {
        var type = testResult.Assembly.GetType("ClassWithExpressionBodiedMember");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() => instance.Method());
        await Assert.That(message.First()).StartsWith("ClassWithExpressionBodiedMember.get_FooBar ");
    }

    [Test]
    public async Task ClassWithAttribute()
    {
        var type = testResult.Assembly.GetType("ClassWithAttribute");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() => instance.Method());
        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("ClassWithAttribute.Method ");
    }

    [Test]
    public async Task MethodWithReturnAndCatchReThrow()
    {
        var type = testResult.Assembly.GetType("MiscMethods");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() => instance.MethodWithReturnAndCatchReThrow());
        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("MiscMethods.MethodWithReturnAndCatchReThrow ");
    }

    [Test]
    public async Task ClassWithMethod()
    {
        var type = testResult.Assembly.GetType("ClassWithMethod");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() => instance.Method());
        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("ClassWithMethod.Method ");
    }

    [Test]
    public async Task GenericClassWithMethod()
    {
        var type = testResult.Assembly.GetType("GenericClassWithMethod`1[[System.String, mscorlib]]");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() => instance.Method());
        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("GenericClassWithMethod`1.Method ");
    }

    [Test]
    public async Task MethodWithReturn()
    {
        var type = testResult.Assembly.GetType("MiscMethods");
        var instance = (dynamic)Activator.CreateInstance(type);
        var message = TraceRunner.Capture(() => instance.MethodWithReturn());
        await Assert.That(message).HasSingleItem();
        await Assert.That(message.First()).StartsWith("MiscMethods.MethodWithReturn ");
    }

    [Test]
    public async Task InheritedClass()
    {
        var type = testResult.Assembly.GetType("InheritedClass");
        var message = TraceRunner.Capture(() => Activator.CreateInstance(type));
        await Assert.That(message).HasSingleItem();
        var first = message.First();
        await Assert.That(first).StartsWith("InheritedClass.ctor ");
    }

    [Test]
    public async Task InheritedClassDoingPropertyCall()
    {
        var type = testResult.Assembly.GetType("InheritedClassDoingPropertyCall");
        var message = TraceRunner.Capture(() => Activator.CreateInstance(type));
        await Assert.That(message).HasSingleItem();
        await Assert.That(message[0]).StartsWith("InheritedClassDoingPropertyCall.ctor ");
    }

    [Test]
    public async Task InheritedClassDoingConstructionCall()
    {
        var type = testResult.Assembly.GetType("InheritedClassDoingConstructionCall");
        var message = TraceRunner.Capture(() => Activator.CreateInstance(type));
        await Assert.That(message).HasSingleItem();
        await Assert.That(message[0]).StartsWith("InheritedClassDoingConstructionCall.ctor ");
    }
}