using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MethodTimer;
#pragma warning disable 414
#pragma warning disable S112

public class ClassWithAsyncMethod
{
    [Time]
    public async Task MethodWithAwaitAsync()
    {
        await Task.Delay(500);
    }

    [Time]
    public async Task MethodWithAwaitAndExceptionAsync()
    {
        await Task.Factory.StartNew(() => throw new Exception("Expected exception"));
    }

    bool isRunning;
    bool isQueued;

    [Time]
    public async Task MethodWithFastPathAsync(bool recurse)
    {
        if (isRunning)
        {
            isQueued = true;
            return;
        }

        isRunning = true;

        await Task.Delay(500);

        if (recurse)
        {
            await MethodWithFastPathAsync(false);
        }

        isRunning = false;
    }

    [Time]
    public async Task MethodWithExceptionAsync()
    {
        await Task.Delay(1000);
        throw new Exception();
    }

    public async Task MethodWithExceptionAsync_Expected()
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await Task.Delay(1000);
            throw new Exception();
        }
        finally
        {
            sw.Stop();
            Trace.WriteLine($"Program.AsyncDelayWithTimer {sw.Elapsed}ms");
        }
    }
}