using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Extensions
{
    internal static class ServiceFactoryExtension
    {
        public static async Task<T> CreateProxyWithRetriesAsync<T>(this IProxyFactory proxyFactory
                , ExecutionIdentity? executionIdentity
                , Func<ExecutionIdentity?, Task<T>> getKeplerServiceWrapper ) where T: class, IDisposable

        {
            int retriesCounter = 0;
            const int retriesLimit = 3;
            Exception proxyException;
            do
            {
                retriesCounter++;
                try
                {
                    return await getKeplerServiceWrapper(executionIdentity);
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
