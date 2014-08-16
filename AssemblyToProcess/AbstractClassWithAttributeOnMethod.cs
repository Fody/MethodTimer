using MethodTimer;

public abstract class AbstractClassWithAttributeOnMethod
{
    [Alternate]
    [Time]
    public abstract void Method();
}

public class GenericClassWithMethod<T> where T : class
 {
     [Alternate]
     [Time]
     public void Method()
     {
     }
 }