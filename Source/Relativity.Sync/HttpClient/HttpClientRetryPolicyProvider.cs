using System;
using System.Net.Http;
using Polly;
using Polly.Retry;
using Relativity.API;
using Relativity.Sync.Utils;

namespace Relativity.Sync.HttpClient
{
    internal class HttpClientRetryPolicyProvider : IHttpClientRetryPolicyProvider
    {
        internal double Pow { get; set; } = 2;

        private readonly ISerializer _serializer;
        private readonly IAPILog _logger;

        public HttpClientRetryPolicyProvider(ISerializer serializer, IAPILog logger)
        {
            _serializer = serializer;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public RetryPolicy<HttpResponseMessage> GetPolicy(int maxRetryCount = 5)
        {
            return SetupPolicyErrorFilters()
                .WaitAndRetryAsync(
                    maxRetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(Pow, retryAttempt)),
                    OnRetry);
        }

        private PolicyBuilder<HttpResponseMessage> SetupPolicyErrorFilters()
        {
            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(responseMessage => !responseMessage.IsSuccessStatusCode);
        }

        private void OnRetry(DelegateResult<HttpResponseMessage> result, TimeSpan timeSpan, int retryCount, Context context)
        {
            string response = result?.Result != null ? _serializer.Serialize(result.Result) : string.Empty;

            _logger.LogWarning(result?.Exception, "Http request failed, retrying... Attempt #{retryCount}. Response message: {@response}", retryCount, response);
        }
    }
}
