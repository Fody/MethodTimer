using System;
using System.Collections.Generic;
using System.Reflection;

public enum InterceptorType
{
    Long,

    TimeSpan
}

public static class MethodTimeLogger
{
    public static readonly List<MethodBase> MethodBase = new List<MethodBase>();
    public static readonly List<string> Messages = new List<string>();
    public static readonly List<string> InterceptorTypes = new List<string>();

    public static void Log(MethodBase methodBase, long milliseconds, string message)
    {
        Console.WriteLine(methodBase.Name + " " + milliseconds + ": " + message);

        MethodBase.Add(methodBase);
        InterceptorTypes.Add(InterceptorType.Long.ToString());

        if (message != null)
        {
            Messages.Add(message);
        }
    }

    public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
    {
        Console.WriteLine(methodBase.Name + " " + elapsed + ": " + message);

        MethodBase.Add(methodBase);
        InterceptorTypes.Add(InterceptorType.TimeSpan.ToString());

        if (message != null)
        {
            Messages.Add(message);
        }
    }
}