using System.Collections.Generic;
using System.Diagnostics;

public class MyTraceListener : TraceListener
{
    public List<string> Messages = new List<string>();

    public override void Write(string message) =>
        Messages.Add(message);

    public override void WriteLine(string message) =>
        Messages.Add(message);
}