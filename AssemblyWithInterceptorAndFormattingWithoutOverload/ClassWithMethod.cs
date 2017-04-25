using System.Threading;
using MethodTimer;

public class ClassWithMethod
{
    [Time("File name: '{fileName}' with id '{id}'")]
    public void Method(string fileName, int id)
    {
        Thread.Sleep(10);
    }
}