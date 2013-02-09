using System;
using System.Collections.Generic;
using System.Diagnostics;

public class  DebugRunner
{

    public static List<string> CaptureDebug(Action action)
    {
        var myTraceListener = new MyTraceListener();
        try
        {
            Debug.Listeners.Add(myTraceListener);
            action();
        }
        finally
        {
            Debug.Listeners.Remove(myTraceListener);
        }
        return myTraceListener.messages;
    }

}