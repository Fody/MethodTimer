using MethodTimer;

[Time]
public class InheritedClass : BaseClass
{
    public InheritedClass()
        : base("message")
    {
    }
}
[Time]
public class InheritedClassDoingCall : BaseClass
{
    public InheritedClassDoingCall()
        : base(Property)
    {
    }

    static string Property
    {
        get { return "aaa"; }
    }
}

[Time]
public class BaseClass
{
    // ReSharper disable once UnusedParameter.Local
    protected BaseClass(string message)
    {

    }
}