using System;
using System.Threading;
using System.Threading.Tasks;
using MethodTimer;

public class ClassWithAsyncMethod
{
    [Time]
    public async void Method()
    {
        Thread.Sleep(10);
    }
    [Time]
    public async Task<DateTime> MethodWithReturn()
    {
        Thread.Sleep(10);
        return DateTime.Now;
    }
}