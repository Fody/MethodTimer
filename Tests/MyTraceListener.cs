using System.Collections.Generic;
using System.Diagnostics;

public class MyTraceListener : TraceListener
{
    public List<string> messages = new List<string>();

    public override void Write(string message)
    {
        messages.Add(message);
    }

    public override void WriteLine(string message)
    {
        messages.Add(message);

    }
}