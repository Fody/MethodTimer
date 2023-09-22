using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MethodTimer;
#pragma warning disable 414

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

    [Time("Current object: '{this}' | File name '{fileName}' with id '{id}'")]
    public async Task MethodWithAwaitAndThisAsync(string fileName, int id)
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
    public async Task MethodWithAwaitButUnusedParametersAsync(string fileName, int id) =>
        await Task.Delay(500);

    [Time("File name '{fileName}' with id '{id}'")]
    public async Task MethodWithAwaitAndExceptionAsync(string fileName, int id)
    {
        // Use so the compiler won't optimize
        Console.Write(fileName);
        Console.Write(id);

        await Task.Factory.StartNew(() => throw new("Expected exception"));
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

    [Time("some message")]
    public async Task<List<T>> MethodWithGenericResultAsync<T>()
    {
        await Task.Delay(50);

        return new();
    }

    public override string ToString() =>
        "TEST VALUE";
}