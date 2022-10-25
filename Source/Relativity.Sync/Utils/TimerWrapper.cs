using System.Threading;

namespace Relativity.Sync.Utils
{
    internal class TimerWrapper : ITimer
    {
        private readonly Timer _timer;

        public TimerWrapper(Timer timer)
        {
            _timer = timer;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
