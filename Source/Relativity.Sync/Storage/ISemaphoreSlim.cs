using System;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
    internal interface ISemaphoreSlim : IDisposable
    {
        void Wait();
        void Wait(CancellationToken cancellationToken);
        Task WaitAsync();
        Task WaitAsync(CancellationToken cancellationToken);
        int Release();
    }
}