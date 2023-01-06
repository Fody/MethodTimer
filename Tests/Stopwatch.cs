using System;

class Stopwatch
{
    long startTicks = CurrentTicks();
    long elapsedTicks;
    bool stopped;

    public static Stopwatch StartNew() => new();

    public void Stop()
    {
        if (!stopped)
        {
            stopped = true;
            elapsedTicks = Math.Max(0, CurrentTicks() - startTicks);
        }
    }

    public bool IsRunning { get => !stopped; }

    static long CurrentTicks() =>
        DateTime.UtcNow.Ticks;

    public long GetElapsedMilliseconds()
    {
        Stop();
        return elapsedTicks / 10000;
    }

    public TimeSpan Elapsed
    {
        get
        {
            Stop();
            return new(elapsedTicks);
        }
    }
}