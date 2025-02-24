namespace ExampleAssembly
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;

    public class MethodsWithTimestampPreweaved
    {
        private long _startTimestamp = default;
        private long _endTimestamp;
        private long _elapsed;
        private TimeSpan _elapsedTimeSpan;
        private int _state = 0;
        private string methodTimerMessage;

        public async Task AsyncMethod()
        {
            if (_startTimestamp == 0)
            {
                _startTimestamp = Stopwatch.GetTimestamp();
            }

            try
            {
                await Task.Delay(5);
            }
            finally
            {
                StopMethodTimerStopwatch();
            }
        }

        private void StopMethodTimerStopwatch()
        {
            if (_state == -2 && _startTimestamp != 0)
            {
                _endTimestamp = Stopwatch.GetTimestamp();
                _elapsed = _endTimestamp - _startTimestamp;
                _elapsedTimeSpan = new TimeSpan((long)(MethodTimerHelper.TimestampToTicks * (double)_elapsed));
                methodTimerMessage = null;
                MethodTimeLogger.Log(MethodBase.GetCurrentMethod(), (long)_elapsedTimeSpan.TotalMilliseconds, methodTimerMessage);
            }
        }
    }
}
