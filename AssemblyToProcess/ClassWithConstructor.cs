using System.Threading;
using MethodTimer;

public class ClassWithConstructor
{
    [Time]
    [Alternate]
    public ClassWithConstructor()
    {
        Thread.Sleep(10);
    }
    [Time]
    [Alternate]
    static ClassWithConstructor()
    {
        Thread.Sleep(10);
    }
}