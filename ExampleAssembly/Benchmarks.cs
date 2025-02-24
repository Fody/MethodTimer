namespace ExampleAssembly
{
    using BenchmarkDotNet.Attributes;

    [MemoryDiagnoser]
    public class StopwatchVsTimestamp
    {
        private TestClassWithStopwatch _testClassWithStopwatch = new TestClassWithStopwatch();
        private TestClassWithTimestamp _testClassWithTimestamp = new TestClassWithTimestamp();

        public StopwatchVsTimestamp()
        {
        }

        [Benchmark]
        public void Timestamp_Sync() => _testClassWithTimestamp.SyncMethod();

        [Benchmark]
        public void Timestamp_Async() => _testClassWithTimestamp.SyncMethod();

        [Benchmark]
        public void Stopwatch_Sync() => _testClassWithStopwatch.SyncMethod();

        [Benchmark]
        public void Stopwatch_Async() => _testClassWithStopwatch.SyncMethod();
    }
}
