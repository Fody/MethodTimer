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
    public async Task MethodWithAwait()
    {
        await Task.Delay(500);
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

}
