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

    public async Task MethodWithAwaitExpected()
    {
        Stopwatch stopwatch = null;
        if (stopwatch == null)
        {
            stopwatch = Stopwatch.StartNew();
        }

        await Task.Delay(500);

        stopwatch.Stop();
        Debug.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");
    }

    [Time]
    public async Task<bool> ComplexMethodWithAwait(int instructionsToHandle)
    {
        if (instructionsToHandle < 0)
        {
            MethodWithException();
        }

        var instructionCounter = 0;
        if (instructionsToHandle <= instructionCounter)
        {
            return false;
        }

        await Task.Delay(200);
        instructionCounter++;

        if (instructionsToHandle <= instructionCounter)
        {
            return false;
        }

        await Task.Delay(200);
        instructionCounter++;

        if (instructionsToHandle <= instructionCounter)
        {
            return false;
        }

        await Task.Delay(100);
        instructionCounter++;

        if (instructionsToHandle <= instructionCounter)
        {
            return false;
        }

        return true;
    }

    public async Task MethodWithException()
    {
        await Task.Factory.StartNew(() =>
        {
            throw new ArgumentOutOfRangeException();
        });
    }

    public async Task<bool> MethodWithAwaitExpected(bool expectedReturn)
    {
        var stopwatch = Stopwatch.StartNew();

        if (expectedReturn)
        {
            stopwatch.Stop();
            Debug.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");

            return false;
        }

        await Task.Delay(500);

        stopwatch.Stop();
        Debug.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");

        return true;
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
