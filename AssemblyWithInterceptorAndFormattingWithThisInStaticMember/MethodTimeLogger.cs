using System;
using System.Collections.Generic;
using System.Reflection;

public static class MethodTimeLogger
{

    public static List<MethodBase> MethodBase = new List<MethodBase>();
    public static List<string> Messages = new List<string>();

    public static void Log(MethodBase methodBase, long milliseconds, string message)
    {
        Console.WriteLine(methodBase.Name + " " + milliseconds + ": " + message);

        MethodBase.Add(methodBase);

        if (message != null)
        {
            Messages.Add(message);
        }
    }

}