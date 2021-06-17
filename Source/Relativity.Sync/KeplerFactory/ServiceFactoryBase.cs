using System;
using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.Sync.KeplerFactory
{
    internal abstract class ServiceFactoryBase
    {
        internal abstract Task<T> CreateProxyInternalAsync<T>() where T : class, IDisposable;

        public async Task<T> CreateProxyAsync<T>() where T : class, IDisposable
        {
            int retriesCounter = 0;
            const int retriesLimit = 3;
            Exception proxyException;
            do
            {
                retriesCounter++;
                try
                {
                    return await CreateProxyInternalAsync<T>().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    proxyException = ex;
                    await Task.Delay(50).ConfigureAwait(false);
                }

            } while (retriesCounter < retriesLimit);

            throw proxyException;
        }
    }
}
