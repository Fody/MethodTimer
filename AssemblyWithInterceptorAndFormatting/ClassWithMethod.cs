using System;
using System.Diagnostics;
using System.Threading;
using MethodTimer;

public class ClassWithMethod
{
    [Time]
    public void MethodWithoutFormatting(string fileName, int id)
    {
        Thread.Sleep(10);
    }

    [Time("File name '{fileName}' with id '{id}'")]
    public void Method(string fileName, int id)
    {
        Thread.Sleep(10);
    }

    public void Method_Expected(string fileName, int id)
    {
        var start = Stopwatch.GetTimestamp();
        try
        {
            Thread.Sleep(10);
        }
        finally
        {
            var end = Stopwatch.GetTimestamp();
            var duration = end - start;
            var elapsedTimeSpan = new TimeSpan((long)(MethodTimerHelper.TimestampToTicks * duration));
            var methodTimerMessage = $"File name '{fileName}' with id '{id}'";
            MethodTimeLogger.Log(null, (long)elapsedTimeSpan.TotalMilliseconds, methodTimerMessage);
        }
    }

    [Time("Current object: '{this}' | File name '{fileName}' with id '{id}'")]
    public void MethodWithThis(string fileName, int id)
    {
        Thread.Sleep(10);
    }

    public void MethodWithThis_Expected(string fileName, int id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Thread.Sleep(10);
        }
        finally
        {
            stopwatch.Stop();
            var methodTimerMessage = $"Current object: '{this}' | File name '{fileName}' with id '{id}'";
            MethodTimeLogger.Log(null, stopwatch.ElapsedMilliseconds, methodTimerMessage);
        }
    }

    public override string ToString()
    {
        return "TEST VALUE";
    }
}