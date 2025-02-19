using System;
using MethodTimer;

public class ClassWithLocalMethods
{
    public void MethodWithLocalInstanceMethod()
    {
        var obj = new object();
        LocalInstanceMethodToTime();

        [Time]
        void LocalInstanceMethodToTime() =>
            GC.KeepAlive(obj);
    }

    public void MethodWithLocalStaticMethod()
    {
        LocalStaticMethodToTime();

        [Time]
        static void LocalStaticMethodToTime()
        {
        }
    }
}
