using System;

namespace Relativity.Sync.Utils
{
    internal sealed class AppDomainWrapper : IAppDomain
    {
        public event UnhandledExceptionEventHandler UnhandledException
        {
            add => AppDomain.CurrentDomain.UnhandledException += value;
            remove => AppDomain.CurrentDomain.UnhandledException -= value;
        }
    }
}
