using System;
using System.Collections.Generic;
using System.Reflection;

namespace AssemblyToReference;

public static class MethodTimeLogger
{
    public static List<MethodBase> MethodBase = new();

    public static void Log(MethodBase methodBase, long milliseconds)
    {
        Console.WriteLine($"{methodBase.Name} {milliseconds}");
        MethodBase.Add(methodBase);
    }
}