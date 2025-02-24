namespace ExampleAssembly;

using BenchmarkDotNet.Running;

public class Program
{
    public static void Main()
    {
        var summary = BenchmarkRunner.Run<StopwatchVsTimestamp>();
    }
}