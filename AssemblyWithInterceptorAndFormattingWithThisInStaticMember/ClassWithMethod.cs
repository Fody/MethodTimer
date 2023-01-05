using System.Threading;
using MethodTimer;

public static class ClassWithMethod
{
    [Time("File name: '{this}' with id '{id}'")]
    public static void Method(string fileName, int id) =>
        Thread.Sleep(10);
}