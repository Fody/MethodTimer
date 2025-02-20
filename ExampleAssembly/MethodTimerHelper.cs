using System;
using System.Diagnostics;

internal static class MethodTimerHelper
{
    internal static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
}