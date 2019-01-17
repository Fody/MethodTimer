using System.Diagnostics;
using System.Threading;
using MethodTimer;

public class ClassWithMethod
{
    [Time]
    public void MethodWithoutFormatting(string fileName, int id)
    {
        Thread.Sleep(10);
    }

    [Time("File name '{fileName}' with id '{id}'")]
    public void Method(string fileName, int id)
    {
        Thread.Sleep(10);
    }

    public void Method_Expected(string fileName, int id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Thread.Sleep(10);
        }
        finally
        {
            stopwatch.Stop();
            var methodTimerMessage = string.Format("File name '{0}' with id '{1}'", new object[] { fileName, id });
            MethodTimeLogger.Log(null, stopwatch.ElapsedMilliseconds, methodTimerMessage);
        }
    }

    [Time("Current object: '{this}' | File name '{fileName}' with id '{id}'")]
    public void MethodWithThis(string fileName, int id)
    {
        Thread.Sleep(10);
    }

    public void MethodWithThis_Expected(string fileName, int id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Thread.Sleep(10);
        }
        finally
        {
            stopwatch.Stop();
            var methodTimerMessage = string.Format("Current object: '{0}' | File name '{1}' with id '{2}'", new object[] { this, fileName, id });
            MethodTimeLogger.Log(null, stopwatch.ElapsedMilliseconds, methodTimerMessage);
        }
    }

    public override string ToString()
    {
        return $"TEST VALUE";
    }
}