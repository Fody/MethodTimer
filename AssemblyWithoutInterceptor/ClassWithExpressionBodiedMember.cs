using System.Diagnostics;
using MethodTimer;

[Time]
public class ClassWithExpressionBodiedMember
{
    public void Method()
    {
        var fooBar = FooBar;
        Trace.WriteLine(fooBar);
    }

    public bool FooBar => Inner is {Foobar: true};
    public Bar Inner;

    public class Bar
    {
        public bool Foobar;
    }
}