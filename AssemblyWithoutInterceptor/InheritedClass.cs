using MethodTimer;
// ReSharper disable UnusedMember.Local

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

    static string Property => "aaa";
}
[Time]
public class InheritedClassDoingConstructionCall : BaseClass
{
    public InheritedClassDoingConstructionCall()
        : base(new SomeOtherClass().Instance)
    {
    }

    static string Property => "aaa";
}

public class SomeOtherClass
{
    public string Instance => "aaa";
    public static string Static => "aaa";
}

public class BaseClass
{
    // ReSharper disable once UnusedParameter.Local
    protected BaseClass(string message)
    {

    }
}