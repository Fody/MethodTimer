using System;
using System.Diagnostics;
using System.Threading.Tasks;

public class TestClass
{
    public void SyncMethod()
    {
        long startTimestamp;
        long endTimestamp;
        long elapsed;
        TimeSpan elapsedTimeSpan;

        startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            // Do something
            Console.WriteLine("Hello, World!");
        }
        finally
        {
            endTimestamp = Stopwatch.GetTimestamp();

            elapsed = endTimestamp - startTimestamp;
            elapsedTimeSpan = new TimeSpan((long)(MethodTimerHelper.TimestampToTicks * elapsed));
        }
    }

    public async Task AsyncMethod()
    {
        long startTimestamp;
        long endTimestamp;
        long elapsed;
        TimeSpan elapsedTimeSpan;

        startTimestamp = Stopwatch.GetTimestamp();
        try
        {
            await Task.Delay(10);
        }
        finally
        {
            endTimestamp = Stopwatch.GetTimestamp();

            elapsed = endTimestamp - startTimestamp;
            elapsedTimeSpan = new TimeSpan((long)(MethodTimerHelper.TimestampToTicks * elapsed));
        }
    }
}