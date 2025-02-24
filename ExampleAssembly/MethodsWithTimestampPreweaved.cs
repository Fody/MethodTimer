namespace ExampleAssembly
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;

    public class MethodsWithTimestampPreweaved
    {
        long _startTimestamp = 0;
        long _endTimestamp;
        long _elapsed;
        TimeSpan _elapsedTimeSpan;
        int _state = 0;
        string methodTimerMessage;

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

        void StopMethodTimerStopwatch()
        {
            if (_state == -2 && _startTimestamp != 0)
            {
                _endTimestamp = Stopwatch.GetTimestamp();
                _elapsed = _endTimestamp - _startTimestamp;
                _elapsedTimeSpan = new TimeSpan((long)(MethodTimerHelper.TimestampToTicks * _elapsed));
                methodTimerMessage = null;
                MethodTimeLogger.Log(MethodBase.GetCurrentMethod(), (long)_elapsedTimeSpan.TotalMilliseconds, methodTimerMessage);
            }
        }
    }
}
