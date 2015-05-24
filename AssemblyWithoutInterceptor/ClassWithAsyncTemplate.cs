using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class ClassWithAsyncTemplate
{

    public async Task MethodWithAwaitExpected()
    {
        Stopwatch stopwatch = null;
        if (stopwatch == null)
        {
            stopwatch = Stopwatch.StartNew();
        }

        await Task.Delay(500);

        stopwatch.Stop();
        Debug.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");
    }

    public async Task<bool> MethodWithAwaitExpected(bool expectedReturn)
    {
        var stopwatch = Stopwatch.StartNew();

        if (expectedReturn)
        {
            stopwatch.Stop();
            Debug.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");

            return false;
        }

        await Task.Delay(500);

        stopwatch.Stop();
        Debug.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");

        return true;
    }

    public async void MethodWithThreadSleep()
    {
        Thread.Sleep(2222);
    }
    public async void MethodWithThreadSleepExpected()
    {
        var stopwatch = Stopwatch.StartNew();

        Thread.Sleep(2222);

        stopwatch.Stop();
        Debug.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.ElapsedMilliseconds + "ms");
    }
}
