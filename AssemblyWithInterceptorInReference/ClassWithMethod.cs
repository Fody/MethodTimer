using System.Threading;
using MethodTimer;

public class ClassWithMethod
{
    [Time]
    public void Method() =>
        Thread.Sleep(10);
}