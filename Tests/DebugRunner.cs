using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public class DebugRunner
{
    public static List<string> CaptureDebug(Action action)
    {
        var myTraceListener = new MyTraceListener();
        try
        {
            Debug.Listeners.Add(myTraceListener);
            action();
            Thread.Sleep(100);
        }
        finally
        {
            Debug.Listeners.Remove(myTraceListener);
        }
        return myTraceListener.Messages;
    }

}