using System.Threading;
using Relativity.API;

namespace kCura.IntegrationPoints.Common.Helpers
{
    public class TimerWrapper : ITimer
    {
        private readonly Timer _timer;
        private readonly string _name;
        private readonly IAPILog _logger;

        public TimerWrapper(Timer timer, string name, IAPILog logger)
        {
            _timer = timer;
            _name = name;
            _logger = logger;
        }

        public bool Change(int dueTime, int period)
        {
            return _timer.Change(dueTime, period);
        }

        public void Dispose()
        {
            _timer.Dispose();
            _logger.LogInformation("Disposed timer name: {name}", _name);
        }
    }
}