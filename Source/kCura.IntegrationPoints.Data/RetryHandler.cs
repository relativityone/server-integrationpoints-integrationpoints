using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Domain.Extensions;
using Polly;
using Polly.Retry;
using Polly.Wrap;
using Relativity.API;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Data
{
    public class RetryHandler : IRetryHandler
    {
        private const ushort _DEFAULT_MAX_NUMBER_OF_RETRIES = 3;
        private const ushort _DEFAULT_EXPONENTIAL_WAIT_TIME_BASE_IN_SEC = 2;
        private const string _CALLER_NAME_KEY = "CallerName";
        private const int _secondsBetweenSqlRetriesBase = 4;
        private readonly ushort _maxNumberOfRetries;
        private readonly ushort _exponentialWaitTimeBaseInSeconds;
        private readonly IAPILog _logger;
        private readonly Random _random;

        public RetryHandler(IAPILog logger) : this(logger, _DEFAULT_MAX_NUMBER_OF_RETRIES, _DEFAULT_EXPONENTIAL_WAIT_TIME_BASE_IN_SEC)
        {
        }

        public RetryHandler(IAPILog logger, ushort maxNumberOfRetries, ushort exponentialWaitTimeBaseInSeconds)
        {
            _maxNumberOfRetries = maxNumberOfRetries;
            _exponentialWaitTimeBaseInSeconds = exponentialWaitTimeBaseInSeconds;
            _logger = logger?.ForContext<RetryHandler>();

            _random = new Random((int)DateTimeOffset.Now.ToUnixTimeSeconds());
        }

        public Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> function, [CallerMemberName] string callerName = "")
        {
            return CreateAsyncRetryPolicy()
                .ExecuteAsync(
                    context => function(),
                    CreateContextData(callerName));
        }

        public Task ExecuteWithRetriesAsync(Func<Task> function, [CallerMemberName] string callerName = "")
        {
            return CreateAsyncRetryPolicy()
                .ExecuteAsync(
                    context => function(),
                    CreateContextData(callerName));
        }

        public T ExecuteWithRetries<T>(Func<T> function, [CallerMemberName] string callerName = "")
        {
            return CreateRetryPolicy()
                .Execute(
                    (context, token) => function(),
                    CreateContextData(callerName),
                    CancellationToken.None);
        }

        public void ExecuteWithRetries(Action action, [CallerMemberName] string callerName = "")
        {
            CreateRetryPolicy()
                .Execute(
                    (context, token) => action(),
                    CreateContextData(callerName),
                    CancellationToken.None);
        }

        public T Execute<T, TException>(Func<T> function, Action<TException> onRetry)
            where TException : Exception
        {
            RetryPolicy genericPolicy = HandleGenericException<TException>()
                .WaitAndRetry(
                    _maxNumberOfRetries,
                    CalculateSleepTimeForHttpException,
                    (Exception exception, TimeSpan waitTime) => onRetry(exception as TException));

            return genericPolicy
                .Execute(
                    token => function(),
                    CancellationToken.None);
        }

        private Dictionary<string, object> CreateContextData(string callerName)
        {
            return new Dictionary<string, object>
            {
                [_CALLER_NAME_KEY] = callerName
            };
        }

        private PolicyWrap CreateRetryPolicy()
        {
            RetryPolicy sqlPolicy = HandleSQLException()
                .WaitAndRetry(
                    _maxNumberOfRetries,
                    CalculateSleepTimeForSQLException,
                    OnRetry);

            RetryPolicy genericPolicy = HandleGenericException<Exception>()
                .WaitAndRetry(
                    _maxNumberOfRetries,
                    CalculateSleepTimeForHttpException,
                    OnRetry);

            PolicyWrap wrapperPolicies = Policy.Wrap(sqlPolicy, genericPolicy);
            return wrapperPolicies;
        }

        private PolicyWrap CreateAsyncRetryPolicy()
        {
            RetryPolicy httpPolicy = HandleGenericException<Exception>()
                .WaitAndRetryAsync(
                    _maxNumberOfRetries,
                    CalculateSleepTimeForHttpException,
                    onRetry: OnRetry);

            RetryPolicy sqlPolicy = HandleSQLException()
                .WaitAndRetryAsync(
                    _maxNumberOfRetries,
                    CalculateSleepTimeForSQLException,
                    onRetry: OnRetry);

            PolicyWrap wrapperPolicies = Policy.WrapAsync(sqlPolicy, httpPolicy);
            return wrapperPolicies;
        }

        private PolicyBuilder HandleGenericException<TException>()
            where TException : Exception
        {
            return Policy
                .Handle<TException>();
        }

        private PolicyBuilder HandleSQLException()
        {
            return Policy
                .Handle<TemporarilyUnavailableException>()
                .Or<kCura.Data.RowDataGateway.TemporarilyUnavailableException>()
                .Or<Exception>(e => e.HasInnerException<TemporarilyUnavailableException>())
                .Or<Exception>(e => e.HasInnerException<kCura.Data.RowDataGateway.TemporarilyUnavailableException>());
        }

        private TimeSpan CalculateSleepTimeForHttpException(int retryAttempt)
        {
            return CalculateSleepTime(retryAttempt, _exponentialWaitTimeBaseInSeconds);
        }

        private TimeSpan CalculateSleepTimeForSQLException(int retryAttempt)
        {
            return CalculateSleepTime(retryAttempt, _secondsBetweenSqlRetriesBase);
        }

        private TimeSpan CalculateSleepTime(int retryAttempt, int secondsBetweenRetriesBase)
        {
            const int maxJitterMs = 100;
            TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(secondsBetweenRetriesBase, retryAttempt));
            TimeSpan jitter = TimeSpan.FromMilliseconds(_random.Next(0, maxJitterMs));
            return delay + jitter;
        }

        private void OnRetry(Exception exception, TimeSpan waitTime, int retryCount, Context context)
        {
            string callerName = GetCallerName(context);

            _logger?.LogWarning(
                exception,
                "Requested operation failed. Caller: {callerName}, retryCount: {retryCount}, waitTime: {waitTime}, correlationId: {policyKey}",
                callerName,
                retryCount,
                waitTime.TotalSeconds,
                context.PolicyKey);
        }

        private string GetCallerName(IDictionary<string, object> contextData)
        {
            return contextData?.ContainsKey(_CALLER_NAME_KEY) == true
                ? contextData[_CALLER_NAME_KEY] as string
                : string.Empty;
        }
    }
}
