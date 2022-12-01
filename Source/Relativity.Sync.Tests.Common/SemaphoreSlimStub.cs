using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Common
{
    [ExcludeFromCodeCoverage]
    internal sealed class SemaphoreSlimStub : ISemaphoreSlim
    {
        private readonly Action _actionBeforeRelease;
        private readonly ISemaphoreSlim _semaphoreSlim = new SemaphoreSlimWrapper(new SemaphoreSlim(1));

        public SemaphoreSlimStub(Action actionBeforeRelease)
        {
            _actionBeforeRelease = actionBeforeRelease;
        }

        public void Dispose()
        {
            _semaphoreSlim.Dispose();
        }

        public void Wait()
        {
            _semaphoreSlim.Wait();
        }

        public void Wait(CancellationToken cancellationToken)
        {
            _semaphoreSlim.Wait(cancellationToken);
        }

        public Task WaitAsync()
        {
            return _semaphoreSlim.WaitAsync();
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            return _semaphoreSlim.WaitAsync(cancellationToken);
        }

        public int Release()
        {
            _actionBeforeRelease();
            return _semaphoreSlim.Release();
        }
    }
}
