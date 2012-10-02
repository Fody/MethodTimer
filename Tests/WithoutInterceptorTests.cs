using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

[TestFixture]
public class WithoutInterceptorTests
{
    Assembly assembly;

    public WithoutInterceptorTests()
    {
        var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\DebugWithoutInterceptor\AssemblyToProcess.dll");
        assembly = AssemblyWeaver.Weave(assemblyPath);
    }

    [Test]
    public void AssertAttributeIsRemoved()
    {
        var type = assembly.GetType("TimeAttribute");
        Assert.IsNull(type);
    }


[Test]
    public void ClassWithConstructor()
    {
        var message = DebugRunner.CaptureDebug(() =>
            {
                var type = assembly.GetType("ClassWithConstructor");
                Activator.CreateInstance(type);
            });
        Assert.IsTrue(message.StartsWith("ClassWithConstructor.ctor "));
    }
    [Test]
    public void ClassWithAttribute()
    {
        var message = DebugRunner.CaptureDebug(() =>
            {
                var type = assembly.GetType("ClassWithAttribute");
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
                var type = assembly.GetType("ClassWithMethod");
                var instance = (dynamic) Activator.CreateInstance(type);
                instance.Method();
            });
        Assert.IsTrue(message.StartsWith("ClassWithMethod.Method "));

    }

    [Test]
    public void MethodWithReturn()
    {
        var message = DebugRunner.CaptureDebug(() =>
            {
                var type = assembly.GetType("MiscMethods");
                var instance = (dynamic) Activator.CreateInstance(type);
                instance.MethodWithReturn();
            });
        Assert.IsTrue(message.StartsWith("MiscMethods.MethodWithReturn "));

    }

#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assembly.CodeBase.Remove(0, 8));
    }
#endif

}