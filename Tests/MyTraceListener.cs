using System.Diagnostics;

public class MyTraceListener : TraceListener
{
    public string message;

    public override void Write(string message)
    {
        this.message = message;
    }

    public override void WriteLine(string message)
    {
        this.message = message;
    }
}