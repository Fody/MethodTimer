namespace ExampleAssembly;

using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class StopwatchVsTimestamp
{
    TestClassWithStopwatch _testClassWithStopwatch = new();
    TestClassWithTimestamp _testClassWithTimestamp = new();

    [Benchmark]
    public void Timestamp_Sync() => _testClassWithTimestamp.SyncMethod();

    [Benchmark]
    public void Timestamp_Async() => _testClassWithTimestamp.SyncMethod();

    [Benchmark]
    public void Stopwatch_Sync() => _testClassWithStopwatch.SyncMethod();

    [Benchmark]
    public void Stopwatch_Async() => _testClassWithStopwatch.SyncMethod();
}