using System;

class Stopwatch
{
    long startTicks;
    long elapsedTicks;
    bool stopped;

    public Stopwatch()
    {
        startTicks = CurrentTicks();
    }

    public static Stopwatch StartNew() => new();

    public void Stop()
    {
        if (!stopped)
        {
            stopped = true;
            elapsedTicks = Math.Max(0, CurrentTicks() - startTicks);
        }
    }

    static long CurrentTicks() =>
        DateTime.UtcNow.Ticks;

    public long GetElapsedMilliseconds()
    {
        Stop();
        return elapsedTicks / 10000;
    }
}