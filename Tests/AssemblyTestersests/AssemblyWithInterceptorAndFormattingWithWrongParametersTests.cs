using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class AssemblyWithInterceptorAndFormattingWithWrongParametersTests
{
    AssemblyWeaver assemblyWeaver;
    string beforeAssemblyPath;

    [Test]
    public void RaisesErrorForWrongParameterNames()
    {
        beforeAssemblyPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\AssemblyWithInterceptorAndFormattingWithWrongParameters\bin\Debug\AssemblyWithInterceptorAndFormattingWithWrongParameters.dll");
        assemblyWeaver = new AssemblyWeaver(beforeAssemblyPath);

        Assert.AreNotEqual(0, assemblyWeaver.Errors);

        var error = assemblyWeaver.Errors.First();
        Assert.AreEqual("Could not process 'System.Void ClassWithMethod::Method(System.String,System.Int32)' because the format uses 'nonExistingParameterName' which is not available as method parameter.", error);
    }
}
