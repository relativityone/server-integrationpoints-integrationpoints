using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
    internal sealed class SemaphoreSlimWrapper : ISemaphoreSlim
    {
        private readonly SemaphoreSlim _semaphoreSlim;

        public SemaphoreSlimWrapper(SemaphoreSlim semaphoreSlim)
        {
            _semaphoreSlim = semaphoreSlim;
        }

        public SemaphoreSlimWrapper()
        {
            _semaphoreSlim = new SemaphoreSlim(1);
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
            return _semaphoreSlim.Release();
        }
    }
}
