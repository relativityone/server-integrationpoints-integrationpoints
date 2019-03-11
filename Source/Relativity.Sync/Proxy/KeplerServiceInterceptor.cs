using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Relativity.Sync.Telemetry;
using SexyProxy;

namespace Relativity.Sync.Proxy
{
	internal sealed class KeplerServiceInterceptor : IProxy
	{
		private const int _NUMBER_OF_RETRIES = 3;
		private const int _MS_BETWEEN_RETRIES = 500;

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

			CommandExecutionStatus status = CommandExecutionStatus.Completed;
			_stopwatch.Start();
			try
			{
				return await Policy
					.Handle<HttpRequestException>() //Thrown when remote endpoint cannot be resolved - connection error
					.WaitAndRetryAsync(_NUMBER_OF_RETRIES, (i, c) => TimeSpan.FromMilliseconds(_MS_BETWEEN_RETRIES), OnRetry)
					.ExecuteAsync(async () => await invocation.Proceed().ConfigureAwait(false)).ConfigureAwait(false);
			}
			catch (Exception)
			{
				status = CommandExecutionStatus.Failed;
				throw;
			}
			finally
			{
				_stopwatch.Stop();
				try
				{
					_syncMetrics.TimedOperation(GetMetricName(invocation), _stopwatch.Elapsed, status);
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Reporting metrics during interception failed.");
				}
			}
		}

		private Task OnRetry(Exception arg1, TimeSpan arg2, Context arg3)
		{
			return Task.CompletedTask;
		}

		private static string GetMetricName(Invocation invocation)
		{
			return $"{invocation.Method.ReflectedType?.FullName}.{invocation.Method.Name}";
		}
	}
}