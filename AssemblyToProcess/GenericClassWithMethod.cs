using System.Threading;
using MethodTimer;

public class GenericClassWithMethod<T> where T : class
{
    [Time]
    public void Method()
    {
        System.Diagnostics.Debug.WriteLine(typeof(T).Name);
        Thread.Sleep(10);
    }
}