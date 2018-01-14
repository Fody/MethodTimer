using System;

class Stopwatch
{
    long startTicks = CurrentTicks();
    const long TicksPerMillisecond = 10000;
    long elapsedTicks;
    bool stopped;

    public static Stopwatch StartNew()
    {
        return new Stopwatch();
    }

    public void Stop()
    {
        if (!stopped)
        {
            stopped = true;
            elapsedTicks = Math.Max(0, CurrentTicks() - startTicks);
        }
    }

    static long CurrentTicks()
    {
        return DateTime.UtcNow.Ticks;
    }

    public long ElapsedMilliseconds
    {
        get
        {
            Stop();
            return elapsedTicks / TicksPerMillisecond;
        }
    }
}