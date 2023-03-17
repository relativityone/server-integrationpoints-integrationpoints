using System;
using System.Threading.Tasks;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Polly.Wrap;
using Relativity.API;
using Relativity.Services.Exceptions;

namespace Relativity.Sync.KeplerFactory
{
    internal abstract class ServiceFactoryBase
    {
        protected readonly IAPILog Logger;

        protected int RetryMaxCount = 2;
        protected int AuthTokenRetriesMaxCount = 2;

        internal TimeSpan TimeBetweenRetries { get; set; } = TimeSpan.FromSeconds(2);

        protected ServiceFactoryBase(IAPILog logger)
        {
            Logger = logger;
        }

        public async Task<T> CreateProxyAsync<T>() where T : class, IDisposable
        {
            RetryPolicy errorsPolicy = GetErrorsPolicy();
            RetryPolicy authTokenPolicy = GetAuthenticationTokenPolicy();
            PolicyWrap wrappedPolicy = Policy.WrapAsync(errorsPolicy, authTokenPolicy);
            T proxy = await wrappedPolicy.ExecuteAsync(
                    async () => await CreateProxyInternalAsync<T>().ConfigureAwait(false))
                .ConfigureAwait(false);

            return proxy;
        }

        protected abstract Task<T> CreateProxyInternalAsync<T>() where T : class, IDisposable;

        private RetryPolicy GetErrorsPolicy()
        {
            RetryPolicy errorsPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeBetweenRetries, RetryMaxCount),
                    (ex, waitTime, retryCount, context) =>
                    {
                        Logger.LogWarning(
                            ex,
                            $"Encountered error for {nameof(CreateProxyInternalAsync)}, attempting retry. Retry count: {retryCount} Wait time: {waitTime.TotalMilliseconds} (ms)");
                    });

            return errorsPolicy;
        }

        private RetryPolicy GetAuthenticationTokenPolicy()
        {
            RetryPolicy authTokenPolicy = Policy
                .Handle<NotAuthorizedException>() // Thrown when token expired
                .RetryAsync(
                    AuthTokenRetriesMaxCount,
                    (ex, retryCount, context) =>
                {
                    Logger.LogWarning(ex, $"Auth token has expired for {nameof(CreateProxyInternalAsync)}, attempting to generate new token and retry. Retry count: {retryCount}");
                });

            return authTokenPolicy;
        }
    }
}
