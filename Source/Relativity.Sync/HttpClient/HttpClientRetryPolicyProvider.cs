using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Relativity.API;

namespace Relativity.Sync.HttpClient
{
    internal class HttpClientRetryPolicyProvider : IHttpClientRetryPolicyProvider
    {
        private readonly IAPILog _logger;

        public HttpClientRetryPolicyProvider(IAPILog logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public RetryPolicy<HttpResponseMessage> GetPolicy(int maxRetryCount = 5)
        {
            return SetupPolicyErrorFilters()
                .WaitAndRetryAsync(
                    maxRetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (result, timeSpan, retryCount, context) => LogRetryMessage(result, retryCount));
        }

        private PolicyBuilder<HttpResponseMessage> SetupPolicyErrorFilters()
        {
            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(msg => !msg.IsSuccessStatusCode);
        }

        private void LogRetryMessage(DelegateResult<HttpResponseMessage> result, int retryCount)
        {
            if (result == null)
            {
                _logger.LogWarning($"Http request failed, retrying... Attempt #{retryCount}.");
                return;
            }

            if (result.Exception != null)
            {
                _logger.LogWarning(result.Exception, BuildErrorDetailsMessage(result.Exception, retryCount));
            }

            if (result.Result != null)
            {
                _logger.LogWarning(BuildErrorDetailsMessage(result.Result, retryCount));
            }
        }

        private static string BuildErrorDetailsMessage(Exception exception, int retryCount)
        {
            var properties = new List<KeyValuePair<string, object>>();
            properties.Add(new KeyValuePair<string, object>("Exception.Message", exception.Message));
            properties.Add(new KeyValuePair<string, object>("Exception.Source", exception.Source));

            string errorDetails = JsonConvert.SerializeObject(properties);
            return $"Http request failed, retrying... Attempt #{retryCount}. Error details: {errorDetails}";
        }

        private static string BuildErrorDetailsMessage(HttpResponseMessage response, int retryCount)
        {
            string errorDetails = JsonConvert.SerializeObject(response);
            return $"Http request failed, retrying... Attempt #{retryCount}. Error details: {errorDetails}";
        }
    }
}
