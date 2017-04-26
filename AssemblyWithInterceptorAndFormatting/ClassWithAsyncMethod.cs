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

        // Use so the compiler won't optimize
        Console.Write(fileName);
        Console.Write(id);
    }

    [Time]
    public async Task MethodWithAwaitWithoutFormattingAsync(string fileName, int id)
    {
        await Task.Delay(500);

        // Use so the compiler won't optimize
        Console.Write(fileName);
        Console.Write(id);
    }

    [Time("File name '{fileName}' with id '{id}'")]
    public async Task MethodWithAwaitButUnusedParametersAsync(string fileName, int id)
    {
        await Task.Delay(500);
    }

    [Time("File name '{fileName}' with id '{id}'")]
    public async Task MethodWithAwaitAndExceptionAsync(string fileName, int id)
    {
        // Use so the compiler won't optimize
        Console.Write(fileName);
        Console.Write(id);

        await Task.Factory.StartNew(() => { throw new Exception("Expected exception"); });
    }

    bool isRunning;
    bool isQueued;

    [Time("File name '{fileName}' with id '{id}'")]
    public async Task MethodWithFastPathAsync(bool recurse, string fileName, int id)
    {
        // Use so the compiler won't optimize
        Console.Write(fileName);
        Console.Write(id);

        if (isRunning)
        {
            isQueued = true;
            return;
        }

        // Use so we 
        Console.WriteLine();

        isRunning = true;

        await Task.Delay(500);

        if (recurse)
        {
            await MethodWithFastPathAsync(false, fileName, id);
        }

        isRunning = false;
    }
}