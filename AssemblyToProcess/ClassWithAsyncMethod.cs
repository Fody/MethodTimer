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
    public async Task MethodWithAwait()
    {
        await Task.Delay(500);
    }

    public async Task MethodWithAwaitOriginal()
    {
        await Task.Delay(500);
    }

    public async Task MethodWithAwaitExpected()
    {
        var stopwatch = Stopwatch.StartNew();

        await Task.Delay(500);

        stopwatch.Stop();
        Debug.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");
    }

    public async void MethodWithThreadSleep()
    {
        Thread.Sleep(2222);
    }
    public async void MethodWithThreadSleepExpected()
    {
        var stopwatch = Stopwatch.StartNew();

        Thread.Sleep(2222);

        stopwatch.Stop();
        Debug.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");
    }
}
