using System.Threading;

public class ClassWithMethod
{
    [Time]
    public void Method()
    {
        Thread.Sleep(10);
    }
}