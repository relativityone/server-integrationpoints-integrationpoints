namespace Relativity.Sync.Utils
{
    internal class TimerFactory : ITimerFactory
    {
        public ITimer Create()
        {
            return new TimerWrapper();
        }
    }
}
