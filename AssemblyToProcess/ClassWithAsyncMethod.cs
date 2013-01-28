using System.Threading;
using MethodTimer;

public class ClassWithAsyncMethod
{
    [Time]
    public async void Method()
    {
        Thread.Sleep(10);
    }
}