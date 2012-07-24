using System.Threading;

namespace AssemblyToProcess
{
    [Time]
    public class ClassWithAttribute
    {
        public void Method()
        {

            Thread.Sleep(1000);
        }
    }
}