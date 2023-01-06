using System;
using System.Threading.Tasks;
using MethodTimer;
#pragma warning disable 414
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

public class ClassWithAsyncMethod
{
    //[Time]
    //public async void MethodWithVoid()
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
    public async Task MethodWithEmptyAsync()
    {

    }

    [Time]
    public async Task MethodWithAwaitAsync() =>
        await Task.Delay(500);

    [Time]
    public async Task MethodWithAwaitAndExceptionAsync() =>
        await Task.Factory.StartNew(() => throw new("Expected exception"));

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
    public async Task<bool> ComplexMethodWithAwaitAsync(int instructionsToHandle)
    {
        if (instructionsToHandle < 0)
        {
            // Note: important not to await
#pragma warning disable 4014
            MethodWithExceptionAsync();
#pragma warning restore 4014
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

    public async Task MethodWithExceptionAsync() =>
        await Task.Factory.StartNew(() => throw new ArgumentOutOfRangeException());
}
