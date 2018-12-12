using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	internal class RetryHandler
	{
		private const int _NUMBER_OF_RETRIES = 3;
		private const int _EXPONENTIAL_WAIT_TIME_BASE = 3;
		private readonly IAPILog _logger;
		private readonly RetryPolicy _retryPolicy;

		public RetryHandler(IAPILog logger)
		{
			_logger = logger.ForContext<RetryHandler>();
			_retryPolicy = CreateRetryPolicy();
		}
		
		public T ExecuteWithRetries<T>(Func<Task<T>> function, [CallerMemberName] string callerName = "")
		{
			return ExecuteWithRetriesAsync(function, callerName)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}

		public async Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> function, [CallerMemberName] string callerName = "")
		{
			return await _retryPolicy
				.ExecuteAsync(
					async () => await function().ConfigureAwait(false),
					false
				)
				.ConfigureAwait(false);
		}

		private RetryPolicy CreateRetryPolicy()
		{
			return Policy
				.Handle<Exception>()
				.WaitAndRetryAsync(
					_NUMBER_OF_RETRIES,
					retryAttempt => TimeSpan.FromSeconds(Math.Pow(_EXPONENTIAL_WAIT_TIME_BASE, retryAttempt)),
					(exception, waitTime, retryCount, context) => OnRetry(exception, waitTime, retryCount, context)
				);
		}

		private void OnRetry(Exception exception, TimeSpan waitTime, int retryCount, Context context)
		{
			string policyKey = context.PolicyKey;
			_logger.LogWarning(exception,
				"ObjectManager request failed. RetryCount: {retryCount}, waitTime: {waitTime}, correlationId: {policyKey}",
				retryCount, waitTime, policyKey);
		}
	}
}
