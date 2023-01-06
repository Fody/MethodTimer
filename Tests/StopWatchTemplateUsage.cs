using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class StopWatchTemplateUsage
{
    public static void Foo()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Thread.Sleep(10);
        }
        finally
        {
            stopwatch.Stop();
            Trace.WriteLine("ClassWithAttribute.Method " + stopwatch.GetElapsedMilliseconds() + "ms");
        }
    }
    public async Task MethodWithAwaitExpected()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await Task.Delay(500);
        }
        finally
        {
            stopwatch.Stop();
            Trace.WriteLine("ClassWithAsyncMethod.MethodWithAwaitExpected " + stopwatch.GetElapsedMilliseconds() + "ms");
        }
    }
}