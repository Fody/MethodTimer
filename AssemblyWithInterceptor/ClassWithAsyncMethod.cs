using System.Threading.Tasks;
using MethodTimer;

public class ClassWithAsyncMethod
{
    [Time]
    public async Task MethodWithAwait()
    {
        await Task.Delay(500);
    }
}