using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Telemetry;
using SexyProxy;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class KeplerServiceInterceptor : IProxy
	{
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
			int attempts = 0;
			_stopwatch.Start();
			try
			{
				const int numberOfRetries = 3;
				object invocationHandler = null;

				while (attempts < numberOfRetries)
				{
					try
					{
						invocationHandler = await invocation.Proceed().ConfigureAwait(false);
						break;
					}
					catch (HttpRequestException)
					{
						attempts++;
						if (attempts == numberOfRetries)
						{
							throw;
						}
						const int msBetweenRetries = 500;
						await Task.Delay(TimeSpan.FromMilliseconds(msBetweenRetries)).ConfigureAwait(false);
					}
				}
				return invocationHandler;
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
					_syncMetrics.TimedOperation(GetMetricName(invocation), _stopwatch.Elapsed, status, CreateCustomData(attempts, exception));
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