using System;
using System.Diagnostics;

public class  DebugRunner
{

    public static string CaptureDebug(Action action)
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
        return myTraceListener.message;
    }

}