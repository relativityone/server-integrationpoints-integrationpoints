using System;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Relativity.Services.Exceptions;
using Relativity.Sync.Utils;

namespace Relativity.Sync.KeplerFactory
{
    internal abstract class ServiceFactoryBase
    {
        protected abstract Task<T> CreateProxyInternalAsync<T>() where T : class, IDisposable;

        public async Task<T> CreateProxyAsync<T>() where T : class, IDisposable
        {
            const int retryCount = 2;
            const int authTokenRetriesCount = 2;
            const double secondsBetweenRetries = 2;

            RetryPolicy errorsPolicy = GetErrorsPolicy(retryCount, secondsBetweenRetries);
            RetryPolicy authTokenPolicy = GetAuthenticationTokenPolicy(authTokenRetriesCount);
            Policy.WrapAsync(errorsPolicy, authTokenPolicy);

            return await errorsPolicy.ExecuteAsync(
                async () => await CreateProxyInternalAsync<T>().ConfigureAwait(false));

        }

        private RetryPolicy GetErrorsPolicy(int retryCount, double secondsBetweenRetries)
        {
            IRandom random = new WrapperForRandom();

            RetryPolicy errorsPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(retryCount, retryAttempt =>
                {
                    const int maxJitterMs = 100;
                    TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(secondsBetweenRetries, retryAttempt));
                    TimeSpan jitter = TimeSpan.FromMilliseconds(random.Next(0, maxJitterMs));
                    return delay + jitter;
                });

            return errorsPolicy;
        }

        private RetryPolicy GetAuthenticationTokenPolicy(int authTokenRetriesCount)
        {
            RetryPolicy authTokenPolicy = Policy
                .Handle<NotAuthorizedException>() // Thrown when token expired
                .RetryAsync(authTokenRetriesCount);

            return authTokenPolicy;
        }
    }
}
