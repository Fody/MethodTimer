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
public class InheritedClassDoingPropertyCall : BaseClass
{
    public InheritedClassDoingPropertyCall()
        : base(SomeOtherClass.Static)
    {
    }

    static string Property
    {
        get { return "aaa"; }
    }
}
[Time]
public class InheritedClassDoingConstructionCall : BaseClass
{
    public InheritedClassDoingConstructionCall()
        : base(new SomeOtherClass().Instance)
    {
    }

    static string Property
    {
        get { return "aaa"; }
    }
}

public class SomeOtherClass
{
    public string Instance { get { return "aaa"; } }
    public static string Static { get { return "aaa"; } }
}

public class BaseClass
{
    // ReSharper disable once UnusedParameter.Local
    protected BaseClass(string message)
    {

    }
}