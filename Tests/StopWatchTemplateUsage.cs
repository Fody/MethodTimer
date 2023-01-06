using System.Threading;

public class StopWatchTemplateUsage
{
    public static long Foo()
    {
        var stopwatch = Stopwatch.StartNew();
        Thread.Sleep(100);

        return stopwatch.GetElapsedMilliseconds();
    }
}