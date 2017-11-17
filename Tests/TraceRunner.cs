using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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

}