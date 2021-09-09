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

        protected readonly IRandom Random;
        protected readonly ISyncLog Logger;
        protected int RetryMaxCount = 2;
        protected int AuthTokenRetriesMaxCount = 2;
        public double SecondsBetweenRetries = 2;

        protected ServiceFactoryBase(IRandom random, ISyncLog logger)
        {
            Random = random;
            Logger = logger;
        }

        protected abstract Task<T> CreateProxyInternalAsync<T>() where T : class, IDisposable;

        public async Task<T> CreateProxyAsync<T>() where T : class, IDisposable
        {

            RetryPolicy errorsPolicy = GetErrorsPolicy();
            RetryPolicy authTokenPolicy = GetAuthenticationTokenPolicy();
            Policy.WrapAsync(errorsPolicy, authTokenPolicy);
            T proxy = await errorsPolicy.ExecuteAsync(
                    async () => await CreateProxyInternalAsync<T>().ConfigureAwait(false))
                .ConfigureAwait(false);

            return proxy;
        }

        private RetryPolicy GetErrorsPolicy()
        {
            RetryPolicy errorsPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(RetryMaxCount, retryAttempt =>
                {
                    const int maxJitterMs = 100;
                    TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(SecondsBetweenRetries, retryAttempt));
                    TimeSpan jitter = TimeSpan.FromMilliseconds(Random.Next(0, maxJitterMs));
                    return delay + jitter;
                },
                (ex, waitTime, retryCount, context) =>
                {
                    Logger.LogWarning(ex,
                        $"Encountered error for {nameof(CreateProxyInternalAsync)}, attempting retry. Retry count: {retryCount} Wait time: {waitTime.TotalMilliseconds} (ms)");
                });

            return errorsPolicy;
        }

        private RetryPolicy GetAuthenticationTokenPolicy()
        {
            RetryPolicy authTokenPolicy = Policy
                .Handle<NotAuthorizedException>() // Thrown when token expired
                .RetryAsync(AuthTokenRetriesMaxCount
                    , (ex, retryCount, context) =>
                {
                    Logger.LogWarning(ex, $"Auth token has expired for {nameof(CreateProxyInternalAsync)}, attempting to generate new token and retry. Retry count: {retryCount}");
                });

            return authTokenPolicy;
        }
    }
}
