using System;
using System.Collections.Generic;
using System.Reflection;

public static class MethodTimeLogger
{
    public static List<MethodBase> MethodBase = new();
    public static List<string> Messages = new();

    public static void Log(MethodBase methodBase, long milliseconds, string message)
    {
        Console.WriteLine(methodBase.Name + " " + milliseconds);
        MethodBase.Add(methodBase);

        if (message != null)
        {
            Messages.Add(message);
        }
    }
}