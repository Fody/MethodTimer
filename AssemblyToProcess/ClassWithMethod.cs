using System.Threading;

public class ClassWithMethod
{
    [Time]
    public void Method()
    {
        Thread.Sleep(10);
    }
    [Time]
    public string MethodWithReturn()
    {
        Thread.Sleep(10);
        return "sd";
    }
}