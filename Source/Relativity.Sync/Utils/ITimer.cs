using System;
using System.Threading;

namespace Relativity.Sync.Utils
{
    internal interface ITimer : IDisposable
    {
        void Activate(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period);
    }
}
