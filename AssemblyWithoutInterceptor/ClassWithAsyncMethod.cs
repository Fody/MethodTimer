using System;
using System.Threading.Tasks;
using MethodTimer;

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
    public async Task MethodWithAwaitAsync()
    {
        await Task.Delay(500);
    }

    [Time]
    public async Task MethodWithAwaitAndExceptionAsync()
    {
        await Task.Factory.StartNew(() => { throw new Exception("Expected exception"); });
    }

    private bool _isRunning;
    private bool _isQueued;

    [Time]
    public async Task MethodWithFastPathAsync(bool recurse)
    {
        if (_isRunning)
        {
            _isQueued = true;
            return;
        }

        _isRunning = true;

        await Task.Delay(500);

        if (recurse)
        {
            await MethodWithFastPathAsync(false);
        }

        _isRunning = false;
    }

    [Time]
    public async Task<bool> ComplexMethodWithAwaitAsync(int instructionsToHandle)
    {
        if (instructionsToHandle < 0)
        {
            // Note: important not to await
            MethodWithExceptionAsync();
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

    public async Task MethodWithExceptionAsync()
    {
        await Task.Factory.StartNew(() =>
        {
            throw new ArgumentOutOfRangeException();
        });
    }

}
