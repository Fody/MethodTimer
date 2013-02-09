using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

    public static class MethodTimeLogger
    {

        public static List<MethodBase> MethodBase = new List<MethodBase>();

        public static void Log(MethodBase methodBase, long milliseconds)
        {
            Debug.WriteLine(methodBase.Name + " " + milliseconds);
            MethodBase.Add(methodBase);
        }

    }