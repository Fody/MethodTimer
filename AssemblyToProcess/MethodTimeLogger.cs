using System.Reflection;

public static class MethodTimeLogger
{
    public static MethodInfo MethodInfo;
    public static void Log(MethodInfo methodInfo, long milliseconds)
    {
        MethodInfo = methodInfo;
    }
}