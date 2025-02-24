using System;
using System.Diagnostics;
using System.Threading.Tasks;

public class TestClassWithStopwatch
{
    public void SyncMethod()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Do something
            //Console.WriteLine("Hello, World!");
        }
        finally
        {
            stopwatch.Stop();

            var elapsedTimeSpan = stopwatch.Elapsed;
        }
    }

    public async Task AsyncMethod()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await Task.Delay(5);
        }
        finally
        {
            stopwatch.Stop();

            var elapsedTimeSpan = stopwatch.Elapsed;
        }
    }
}