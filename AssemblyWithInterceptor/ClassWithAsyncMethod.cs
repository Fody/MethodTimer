using System.Threading.Tasks;
using MethodTimer;

public class ClassWithAsyncMethod
{
    [Time]
    public async Task MethodWithAwaitAsync()
    {
        await Task.Delay(500);
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
}