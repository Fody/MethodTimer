using System.Threading;
using MethodTimer;

[Time]
[Alternate]
public class ClassWithAttribute
{
    public void Method()
    {
        Thread.Sleep(10);
    }
}

//TODO: recursive
//TODO: single return