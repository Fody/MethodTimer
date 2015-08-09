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
    public async Task MethodWithFastPathAsync(bool returnImmediately)
    {
        if (returnImmediately)
        {
            return;
        }

        await MethodWithAwaitAsync();
    }
}