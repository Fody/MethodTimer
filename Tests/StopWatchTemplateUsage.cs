using System;
using System.Diagnostics;
using System.Threading;

public class StopWatchTemplateUsage
{
    public static void Foo()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Thread.Sleep(100);
        }
        finally
        {
            stopwatch.Stop();
            Trace.WriteLine("ClassWithAttribute.Method " + stopwatch.GetElapsedMilliseconds() + "ms");
        }
    }
}