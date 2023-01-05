using System.Collections.Generic;
using MethodTimer;

public class ClassWithYieldMethod
{
    [Time]
    public IEnumerable<string> YieldMethod()
    {
        yield return "a";
        yield return "b";
        yield return "C";
        yield return "X";
    }
}