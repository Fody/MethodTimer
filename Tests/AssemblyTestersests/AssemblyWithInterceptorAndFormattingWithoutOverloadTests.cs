using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class AssemblyWithInterceptorAndFormattingWithoutOverloadTests
{
    AssemblyWeaver assemblyWeaver;
    string beforeAssemblyPath;

    [Test]
    public void RaisesErrorForMissingOverload()
    {
        beforeAssemblyPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\..\AssemblyWithInterceptorAndFormattingWithoutOverload\bin\Debug\net462\AssemblyWithInterceptorAndFormattingWithoutOverload.dll");
        assemblyWeaver = new AssemblyWeaver(beforeAssemblyPath);

        Assert.AreNotEqual(0, assemblyWeaver.Errors);

        var error = assemblyWeaver.Errors.First();
        Assert.AreEqual("Feature with parameter formatting is being used, but no useable log method can be found. Either disable the feature usage or update the logger signature to 'public static void Log(MethodBase methodBase, long milliseconds, string message)'", error);
    }
}
