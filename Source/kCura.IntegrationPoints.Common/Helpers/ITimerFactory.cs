using System;
using System.Threading;

namespace kCura.IntegrationPoints.Common.Helpers
{
    public interface ITimerFactory
    {
        ITimer Create(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period, string name);
    }
}