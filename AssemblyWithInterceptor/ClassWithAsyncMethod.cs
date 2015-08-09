using System;
using System.Threading.Tasks;
using MethodTimer;

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
        await Task.Factory.StartNew(() => { throw new Exception("Expected exception"); });
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
}