using System.Threading;
using MethodTimer;

public class ClassWithConstructor
{
    [Time]
    public ClassWithConstructor()
    {
        Thread.Sleep(10);
    }
    [Time]
    static ClassWithConstructor()
    {
        Thread.Sleep(10);
    }
}
