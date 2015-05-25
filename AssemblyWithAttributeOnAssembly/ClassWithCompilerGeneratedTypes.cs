using System.Collections.Generic;
using System.Threading.Tasks;

public class ClassWithCompilerGeneratedTypes
{
    public IEnumerable<string> YieldMethod()
    {
        yield return "a";
        yield return "b";
    }

    public async Task AsyncMethod()
    {
        await Task.Delay(100);
    }
}