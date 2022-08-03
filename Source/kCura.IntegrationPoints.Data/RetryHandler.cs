using Polly;
using Polly.Retry;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Handlers;

namespace kCura.IntegrationPoints.Data
{
    public class RetryHandler : IRetryHandler
    {
        private const string _CALLER_NAME_KEY = "CallerName";

        private readonly ushort _maxNumberOfRetries;
        private readonly ushort _exponentialWaitTimeBaseInSeconds;
        private readonly IAPILog _logger;
        private readonly RetryPolicy _asyncRetryPolicy;
        private readonly RetryPolicy _retryPolicy;

        public RetryHandler(IAPILog logger, ushort maxNumberOfRetries, ushort exponentialWaitTimeBaseInSeconds)
        {
            _maxNumberOfRetries = maxNumberOfRetries;
            _exponentialWaitTimeBaseInSeconds = exponentialWaitTimeBaseInSeconds;
            _logger = logger?.ForContext<RetryHandler>();
            _asyncRetryPolicy = CreateAsyncRetryPolicy();
            _retryPolicy = CreateRetryPolicy();

        }

        public Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> function, [CallerMemberName] string callerName = "")
        {
            return _asyncRetryPolicy.ExecuteAsync(
                context => function(),
                CreateContextData(callerName)
            );
        }

        public Task ExecuteWithRetriesAsync(Func<Task> function, [CallerMemberName] string callerName = "")
        {
            return _asyncRetryPolicy.ExecuteAsync(
                context => function(), 
                CreateContextData(callerName)
            );
        }

        public T ExecuteWithRetries<T>(Func<T> function, [CallerMemberName] string callerName = "")
        {
            return _retryPolicy.Execute((context, token) => function(), CreateContextData(callerName), CancellationToken.None);
        }

        private Dictionary<string, object> CreateContextData(string callerName)
        {
            return new Dictionary<string, object>
            {
                [_CALLER_NAME_KEY] = callerName
            };
        }

        private RetryPolicy CreateRetryPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetry(_maxNumberOfRetries, CalculateWaitTime, OnRetry);
        }

        private RetryPolicy CreateAsyncRetryPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_maxNumberOfRetries, CalculateWaitTime, onRetry: OnRetry);
        }

        private TimeSpan CalculateWaitTime(int retryAttempt)
        {
            double numberOfSecondsToWait = Math.Pow(_exponentialWaitTimeBaseInSeconds, retryAttempt);
            return TimeSpan.FromSeconds(numberOfSecondsToWait);
        }

        private void OnRetry(Exception exception, TimeSpan waitTime, int retryCount, Context context)
        {
            string callerName = GetCallerName(context);
            string policyKey = context.PolicyKey;

            _logger?.LogWarning(exception,
                "Requested operation failed. Caller: {callerName}, retryCount: {retryCount}, waitTime: {waitTime}, correlationId: {policyKey}",
                callerName, retryCount, waitTime, policyKey);
        }

        private string GetCallerName(IDictionary<string, object> contextData)
        {
            return contextData?.ContainsKey(_CALLER_NAME_KEY) == true
                ? contextData[_CALLER_NAME_KEY] as string
                : string.Empty;
        }
    }
}