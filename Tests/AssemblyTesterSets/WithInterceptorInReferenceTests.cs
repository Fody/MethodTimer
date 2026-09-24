using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fody;

// tests share static state and capture Trace output
[NotInParallel]
public class WithInterceptorInReferenceTests
{
    static FieldInfo methodBaseField;
    static TestResult testResult;

    static WithInterceptorInReferenceTests()
    {
        var weavingTask = new ModuleWeaver
        {
            ReferenceCopyLocalPaths = ["AssemblyToReference.dll"]
        };
        testResult = weavingTask.ExecuteTestRun(
            assemblyPath: "AssemblyWithInterceptorInReference.dll",
            ignoreCodes: ["0x80131869"]);
        methodBaseField = typeof(AssemblyToReference.MethodTimeLogger).GetField("MethodBase");
    }

    [Test]
    public async Task ClassWithMethod()
    {
        ClearMessage();
        var instance = testResult.GetInstance("ClassWithMethod");
        instance.Method();
        var methodBases = GetMethodInfoField();
        await Assert.That(methodBases).HasSingleItem();
        var methodBase = methodBases.First();
        await Assert.That(methodBase.Name).IsEqualTo("Method");
        await Assert.That((object) instance.GetType()).IsEqualTo(methodBase.DeclaringType);
    }

    static void ClearMessage() =>
        methodBaseField.SetValue(null, new List<MethodBase>());

    static List<MethodBase> GetMethodInfoField() =>
        (List<MethodBase>)methodBaseField.GetValue(null);
}