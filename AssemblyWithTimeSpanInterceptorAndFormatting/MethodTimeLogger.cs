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
    public static List<MethodBase> MethodBase = new();
    public static List<string> Messages = new();
    public static List<string> InterceptorTypes = new();

    public static void Log(MethodBase methodBase, long milliseconds, string message)
    {
        Console.WriteLine($"{methodBase.Name} {milliseconds}: {message}");

        MethodBase.Add(methodBase);
        InterceptorTypes.Add(nameof(InterceptorType.Long));

        if (message != null)
        {
            Messages.Add(message);
        }
    }

    public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
    {
        Console.WriteLine($"{methodBase.Name} {elapsed}: {message}");

        MethodBase.Add(methodBase);
        InterceptorTypes.Add(nameof(InterceptorType.TimeSpan));

        if (message != null)
        {
            Messages.Add(message);
        }
    }
}