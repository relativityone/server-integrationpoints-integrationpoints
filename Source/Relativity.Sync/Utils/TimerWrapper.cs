using System;
using System.Threading;

namespace Relativity.Sync.Utils
{
    internal class TimerWrapper : ITimer
    {
        private Timer _timer;

        public void Activate(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            if (_timer != null)
            {
                throw new InvalidOperationException("Previous timer has not been disposed.");
            }

            _timer = new Timer(callback, state, dueTime, period);
        }

        public void Deactivate()
        {
            _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}
