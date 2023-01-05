using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fody;
using Xunit;

public class WithInterceptorInReferenceTests
{
    static FieldInfo methodBaseField;
    static TestResult testResult;

    static WithInterceptorInReferenceTests()
    {
        var weavingTask = new ModuleWeaver
        {
            ReferenceCopyLocalPaths = new List<string>
            {
                "AssemblyToReference.dll"
            }
        };
        testResult = weavingTask.ExecuteTestRun(
            assemblyPath: "AssemblyWIthInterceptorInReference.dll",
            ignoreCodes: new []{ "0x80131869" });
        methodBaseField = typeof(AssemblyToReference.MethodTimeLogger).GetField("MethodBase");
    }

    [Fact]
    public void ClassWithMethod()
    {
        ClearMessage();
        var instance = testResult.GetInstance("ClassWithMethod");
        instance.Method();
        var methodBases = GetMethodInfoField();
        Assert.Single(methodBases);
        var methodBase = methodBases.First();
        Assert.Equal("Method", methodBase.Name);
        Assert.Equal(methodBase.DeclaringType, instance.GetType());
    }

    static void ClearMessage() =>
        methodBaseField.SetValue(null, new List<MethodBase>());

    static List<MethodBase> GetMethodInfoField() =>
        (List<MethodBase>)methodBaseField.GetValue(null);
}