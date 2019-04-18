using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public class MyTraceListener : TraceListener
{
    public static AsyncLocal<List<string>> Messages = new AsyncLocal<List<string>>();

    public override void Write(string message)
    {
        Messages.Value?.Add(message);
    }

    public override void WriteLine(string message)
    {
        Messages.Value?.Add(message);
    }
}