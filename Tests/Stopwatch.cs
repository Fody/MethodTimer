using System;

class Stopwatch
{
    long startTicks = CurrentTicks();
    const long TicksPerMillisecond = 10000;
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

    public long ElapsedMilliseconds
    {
        get
        {
            Stop();
            return elapsedTicks / TicksPerMillisecond;
        }
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