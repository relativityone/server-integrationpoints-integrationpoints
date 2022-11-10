using System;
using System.Threading;
using Relativity.API;

namespace Relativity.Sync.Utils
{
    internal class TimerWrapper : ITimer
    {
        private readonly IAPILog _log;

        private Timer _timer;
        private TimerCallback _timerCallback;

        public TimerWrapper(IAPILog log)
        {
            _log = log;
        }

        public void Activate(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            _log?.LogInformation("Activating Timer...");
            if (_timer != null)
            {
                throw new InvalidOperationException("Previous timer has not been disposed.");
            }

            _timerCallback = callback;

            _timer = new Timer(_timerCallback, state, dueTime, period);
        }

        public void Dispose()
        {
            _log?.LogInformation("Disposing Timer...");
            _timer?.Dispose();
            _timer = null;

            GC.SuppressFinalize(this);
        }
    }
}
