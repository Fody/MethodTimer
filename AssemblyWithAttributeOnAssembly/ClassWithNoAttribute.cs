using System.Threading;

public class ClassWithNoAttribute
{
    public void Method() =>
        Thread.Sleep(10);
}
