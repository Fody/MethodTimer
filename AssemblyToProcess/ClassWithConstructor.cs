using System.Threading;

public class ClassWithConstructor
{
    [Time]
    public ClassWithConstructor()
    {
        Thread.Sleep(10);
    }
}