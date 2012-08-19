using System.Reflection;

    public static class MethodTimeLogger
    {

        public static MethodBase MethodBase;

        public static void Log(MethodBase methodBase, long milliseconds)
        {
            MethodBase = methodBase;
        }

    }