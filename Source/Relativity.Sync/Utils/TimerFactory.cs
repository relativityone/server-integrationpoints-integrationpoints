using Relativity.API;

namespace Relativity.Sync.Utils
{
    internal class TimerFactory : ITimerFactory
    {
        private readonly IAPILog _log;

        public TimerFactory(IAPILog log)
        {
            _log = log;
        }

        public ITimer Create()
        {
            return new TimerWrapper(_log);
        }
    }
}
