using System.IO;
using NUnit.Framework;

[TestFixture]
[Explicit]
public class ExperimentTests
{

    [Test]
    public void Foo()
    {
        var assemblyPath = Path.GetFullPath(@"C:\Code\Nancy\src\Nancy\obj\Debug\Nancy.dll");
        var assemblyWeaver = new AssemblyWeaver(assemblyPath);
        Verifier.Verify(assemblyWeaver.Assembly.CodeBase.Remove(0, 8));
    }
}