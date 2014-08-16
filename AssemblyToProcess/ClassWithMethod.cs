using System.Threading;
using MethodTimer;

public class ClassWithMethod
{
    [Time]
    [Alternate]
    public void Method()
    {
        Thread.Sleep(10);
    }
}