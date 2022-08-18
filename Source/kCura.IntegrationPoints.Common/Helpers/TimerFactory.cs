using Relativity.API;
using System.Threading;
using System;

namespace kCura.IntegrationPoints.Common.Helpers
{
    public class TimerFactory : ITimerFactory
    {
        private readonly IAPILog _logger;

        public TimerFactory(IAPILog logger)
        {
            _logger = logger;
        }

        public ITimer Create(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period, string name)
        {
            Timer timer = new Timer(callback, state, dueTime, period);
            return new TimerWrapper(timer, name, _logger);
        }
    }
}