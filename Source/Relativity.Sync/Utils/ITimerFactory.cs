using System;
using System.Threading;

namespace Relativity.Sync.Utils
{
    internal interface ITimerFactory
    {
        ITimer Create(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period);
    }
}
