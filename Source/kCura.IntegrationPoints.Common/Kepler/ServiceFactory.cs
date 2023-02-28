using System;
using System.Threading.Tasks;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Relativity.API;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Common.Kepler
{
    public class ServiceFactory : IKeplerServiceFactory
    {
        private readonly TimeSpan _waitTime = TimeSpan.FromSeconds(2);
        private readonly IServicesMgr _servicesMgr;
        private readonly IDynamicProxyFactory _proxyFactory;
        private readonly IAPILog _logger;

        private const int RetryMaxCount = 2;
        private const int AuthTokenRetriesMaxCount = 2;

        public ServiceFactory(IServicesMgr servicesMgr, IDynamicProxyFactory proxyFactory, IAPILog logger)
        {
            _servicesMgr = servicesMgr;
            _proxyFactory = proxyFactory;
            _logger = logger;
        }

        public async Task<T> CreateProxyAsync<T>() where T : class, IDisposable
        {
            RetryPolicy errorsPolicy = GetErrorsPolicy();
            RetryPolicy authTokenPolicy = GetAuthenticationTokenPolicy();
            Policy.WrapAsync(errorsPolicy, authTokenPolicy);
            T proxy = await errorsPolicy.ExecuteAsync(async () => await CreateProxyInternalAsync<T>().ConfigureAwait(false)).ConfigureAwait(false);

            return proxy;
        }

        protected async Task<T> CreateProxyInternalAsync<T>() where T : class, IDisposable
        {
            Func<Task<T>> keplerServiceFactory = () => Task.FromResult(_servicesMgr.CreateProxy<T>(ExecutionIdentity.System));
            T keplerService = await keplerServiceFactory().ConfigureAwait(false);
            return _proxyFactory.WrapKeplerService(keplerService, keplerServiceFactory);
        }

        private RetryPolicy GetErrorsPolicy()
        {
            RetryPolicy errorsPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(_waitTime, RetryMaxCount),
                    (ex, waitTime, retryCount, context) =>
                    {
                        _logger.LogWarning(
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
                        _logger.LogWarning(ex, $"Auth token has expired for {nameof(CreateProxyInternalAsync)}, attempting to generate new token and retry. Retry count: {retryCount}");
                    });

            return authTokenPolicy;
        }
    }
}