using System;

namespace Relativity.Sync.Utils
{
    internal interface IAppDomain
    {
        event UnhandledExceptionEventHandler UnhandledException;
    }
}