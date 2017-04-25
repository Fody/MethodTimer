using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        beforeAssemblyPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\AssemblyWithInterceptorAndFormatting\bin\Debug\AssemblyWithInterceptorAndFormatting.dll");
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

    void ClearMessage()
    {
        methodBaseField.SetValue(null, new List<MethodBase>());
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