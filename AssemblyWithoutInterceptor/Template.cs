using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class Template
{

    public async Task MethodWithAwaitExpected()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await Task.Delay(500);
        }
        finally
        {
            stopwatch.Stop();
            Trace.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");
        }
    }
    public void MethodAndCatchReThrow()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {

        try
        {
            Thread.Sleep(10);
        }
        catch (Exception)
        {
                Trace.WriteLine("sdf");
            throw;
        }
        }
        finally
        {

            stopwatch.Stop();
            Trace.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");
        }

    }

    public async Task<bool> MethodWithAwaitExpected(bool expectedReturn)
    {
        var stopwatch = Stopwatch.StartNew();

        if (expectedReturn)
        {
            stopwatch.Stop();
            Trace.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");

            return false;
        }

        await Task.Delay(500);

        stopwatch.Stop();
        Trace.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");

        return true;
    }

    //public async void MethodWithThreadSleep()
    //{
    //    Thread.Sleep(2222);
    //}
    //public async void MethodWithThreadSleepExpected()
    //{
    //    var stopwatch = Stopwatch.StartNew();

    //    Thread.Sleep(2222);

    //    stopwatch.Stop();
    //    Debug.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");
    //}
}
