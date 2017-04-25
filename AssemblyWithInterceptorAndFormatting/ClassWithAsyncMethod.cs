using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MethodTimer;

public class ClassWithAsyncMethod
{
    [Time("File name '{fileName}' with id '{id}'")]
    public async Task MethodWithAwaitAsync(string fileName, int id)
    {
        await Task.Delay(500);
    }

    public async Task MethodWithAwaitAsync_Expected(string fileName, int id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await Task.Delay(500);
        }
        finally
        {
            stopwatch.Stop();
            var methodTimerMessage = string.Format("File name '{0}' with id '{1}'", new object[] { fileName, id });
            MethodTimeLogger.Log(null, stopwatch.ElapsedMilliseconds, methodTimerMessage);
        }
    }

    [Time("File name '{fileName}' with id '{id}'")]
    public async Task MethodWithAwaitAndExceptionAsync(string fileName, int id)
    {
        await Task.Factory.StartNew(() => { throw new Exception("Expected exception"); });
    }

    bool isRunning;
    bool isQueued;

    [Time("File name '{fileName}' with id '{id}'")]
    public async Task MethodWithFastPathAsync(bool recurse, string fileName, int id)
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
            await MethodWithFastPathAsync(false, fileName, id);
        }

        isRunning = false;
    }
}