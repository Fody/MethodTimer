using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

public class TestClassWithTimestamp
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
            //Console.WriteLine("Hello, World!");
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
        long startTimestamp = default;
        long endTimestamp;
        long elapsed;
        TimeSpan elapsedTimeSpan;

        if (startTimestamp == 0)
        {
            startTimestamp = Stopwatch.GetTimestamp();
        }

        try
        {
            await Task.Delay(5);
        }
        finally
        {
            if (startTimestamp != 0)
            {
                endTimestamp = Stopwatch.GetTimestamp();

                elapsed = endTimestamp - startTimestamp;
                elapsedTimeSpan = new((long)(MethodTimerHelper.TimestampToTicks * elapsed));
            }
        }
    }
}