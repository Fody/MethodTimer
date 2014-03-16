using MethodTimer;

public abstract class AbstractClassWithAttributeOnMethod
{
    [Time]
    public abstract void Method();
}

public class GenericClassWithMethod<T> where T : class
 {
     [Time]
     public void Method()
     {
     }
 }