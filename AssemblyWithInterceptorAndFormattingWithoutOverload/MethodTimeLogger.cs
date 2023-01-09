using System;
using System.Collections.Generic;
using System.Reflection;

public static class MethodTimeLogger
{
    public static List<MethodBase> MethodBase = new();

    public static void Log(MethodBase methodBase, long milliseconds)
    {
        Log(methodBase.DeclaringType, methodBase.Name, milliseconds);
        MethodBase.Add(methodBase);
    }

    public static void Log(Type type, string methodName, long milliseconds) =>
        Console.WriteLine($"{methodName} {milliseconds}");
}