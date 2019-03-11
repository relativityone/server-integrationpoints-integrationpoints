using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Relativity.Sync.Telemetry;
using SexyProxy;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class KeplerServiceInterceptor : IProxy
	{
		private const int _NUMBER_OF_RETRIES = 3;
		private const int _MS_BETWEEN_RETRIES = 500;

		private const string _RETRIES_COUNT_METRIC_NAME = "KeplerRetries";
		private const string _EXCEPTION_METRIC_NAME = "KeplerException";

		private readonly ISyncMetrics _syncMetrics;
		private readonly IStopwatch _stopwatch;
		private readonly ISyncLog _logger;

		public KeplerServiceInterceptor(ISyncMetrics syncMetrics, IStopwatch stopwatch, ISyncLog logger)
		{
			_syncMetrics = syncMetrics;
			_stopwatch = stopwatch;
			_logger = logger;
		}

		public InvocationHandler InvocationHandler => new InvocationHandler(Intercept);

		public async Task<object> Intercept(Invocation invocation)
		{
			if (invocation.Method.Name == nameof(IDisposable.Dispose))
			{
				return await invocation.Proceed().ConfigureAwait(false);
			}

			ExecutionStatus status = ExecutionStatus.Completed;
			Exception exception = null;
			int numberOfRetries = 0;
			_stopwatch.Start();
			try
			{
				return await Policy
					.Handle<HttpRequestException>() //Thrown when remote endpoint cannot be resolved - connection error
					.WaitAndRetryAsync(_NUMBER_OF_RETRIES, (i, c) => TimeSpan.FromMilliseconds(_MS_BETWEEN_RETRIES), (e, waitTime, retryCount, context) => numberOfRetries = retryCount)
					.ExecuteAsync(async () => await invocation.Proceed().ConfigureAwait(false)).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				status = ExecutionStatus.Failed;
				exception = e;
				throw;
			}
			finally
			{
				_stopwatch.Stop();
				try
				{
					_syncMetrics.TimedOperation(GetMetricName(invocation), _stopwatch.Elapsed, status, CreateCustomData(numberOfRetries, exception));
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Reporting metrics during interception failed.");
				}
			}
		}

		private static Dictionary<string, object> CreateCustomData(int numberOfRetries, Exception exception)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{_RETRIES_COUNT_METRIC_NAME, numberOfRetries}
			};
			if (exception != null)
			{
				dictionary.Add(_EXCEPTION_METRIC_NAME, exception);
			}

			return dictionary;
		}

		private static string GetMetricName(Invocation invocation)
		{
			return $"{invocation.Method.ReflectedType?.FullName}.{invocation.Method.Name}";
		}
	}
}