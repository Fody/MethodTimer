using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class TraceRunner
{
    public static List<string> Capture(Action action)
    {
        var myTraceListener = new MyTraceListener();
        try
        {
            Trace.Listeners.Add(myTraceListener);
            action();
            Thread.Sleep(100);
        }
        finally
        {
            Trace.Listeners.Remove(myTraceListener);
        }
        return myTraceListener.Messages;
    }

    public static async Task<List<string>> CaptureAsync(Func<Task> action)
    {
        var myTraceListener = new MyTraceListener();
        try
        {
            Trace.Listeners.Add(myTraceListener);
            await action();
            Thread.Sleep(100);
        }
        finally
        {
            Trace.Listeners.Remove(myTraceListener);
        }

        return myTraceListener.Messages;
    }
}