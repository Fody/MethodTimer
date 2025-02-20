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
        long ticks;
        TimeSpan elapsedTimeSpan;

        startTimestamp = Stopwatch.GetTimestamp();

        // Do something

        endTimestamp = Stopwatch.GetTimestamp();

        elapsed = endTimestamp - startTimestamp;
        ticks = (long)(MethodTimerHelper.TimestampToTicks * elapsed);
        elapsedTimeSpan = new TimeSpan(ticks);
    }

    public async Task AsyncMethod()
    {
        long startTimestamp;
        long endTimestamp;
        long elapsed;
        long ticks;
        TimeSpan elapsedTimeSpan;

        startTimestamp = Stopwatch.GetTimestamp();

        await Task.Delay(10);

        endTimestamp = Stopwatch.GetTimestamp();

        elapsed = endTimestamp - startTimestamp;
        ticks = (long)(MethodTimerHelper.TimestampToTicks * elapsed);
        elapsedTimeSpan = new TimeSpan(ticks);
    }
}