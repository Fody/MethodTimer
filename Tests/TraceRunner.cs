using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public static class TraceRunner
{
    static TraceRunner()
    {
        var myTraceListener = new MyTraceListener();
        Trace.Listeners.Add(myTraceListener);
    }

    public static List<string> Capture(Action action)
    {
        var list = new List<string>();
        MyTraceListener.Messages.Value = list;
        action();
        Thread.Sleep(100);
        return list;
    }
}