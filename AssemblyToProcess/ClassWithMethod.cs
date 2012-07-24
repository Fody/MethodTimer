using System.Threading;

namespace AssemblyToProcess
{
    public class ClassWithMethod
    {
        [Time]
        public void Method()
        {
          Thread.Sleep(1000);  
        }
    }
}
