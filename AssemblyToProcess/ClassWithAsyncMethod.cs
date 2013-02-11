using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MethodTimer;

public class ClassWithAsyncMethod
{
    //[Time]
    //public async void Method()
    //{
    //    Thread.Sleep(10);
    //}
    //[Time]
    //public async Task<DateTime> MethodWithReturn()
    //{
    //    Thread.Sleep(2000);
    //    return DateTime.Now;
    //}

    [Time]
    public async void MethodWithAwait()
    {
            await Task.Delay(500);
    }
    public async void MethodWithAwaitExpected()
    {
        var startNew = Stopwatch.StartNew();
        await Task.Delay(500);
        startNew.Stop();
        Console.WriteLine(startNew.ElapsedMilliseconds);
    }
    public async void MethodWithThreadSleep()
    {
        Thread.Sleep(2222);
    }
    public async void MethodWithThreadSleepExpected()
    {
        var startNew = Stopwatch.StartNew();
        Thread.Sleep(2222);
        startNew.Stop();
        Console.WriteLine(startNew.ElapsedMilliseconds);
    }
}
