using System;
using System.Threading;

namespace Relativity.Sync.Utils
{
    internal class TimerFactory : ITimerFactory
    {
        public ITimer Create(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            Timer timer = new Timer(callback, state, dueTime, period);
            return new TimerWrapper(timer);
        }
    }
}
