using System;
using System.Collections.Generic;
using System.Reflection;

public static class MethodTimeLogger
{

    public static List<MethodBase> MethodBase = new List<MethodBase>();

    public static void Log(MethodBase methodBase, long milliseconds)
    {
        Console.WriteLine(methodBase.Name + " " + milliseconds);
        MethodBase.Add(methodBase);
    }

}