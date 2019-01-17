using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync
{
	internal sealed class CommandWithMetrics<T> : ICommand<T> where T : IConfiguration
	{
		private readonly ICommand<T> _innerCommand;
		private readonly ISyncMetrics _metrics;
		private readonly IStopwatch _stopwatch;

		public CommandWithMetrics(ICommand<T> innerCommand, ISyncMetrics metrics, IStopwatch stopwatch)
		{
			_innerCommand = innerCommand;
			_metrics = metrics;
			_stopwatch = stopwatch;
		}

		public async Task<bool> CanExecuteAsync(CancellationToken token)
		{
			return await _innerCommand.CanExecuteAsync(token).ConfigureAwait(false);
		}

		public async Task ExecuteAsync(CancellationToken token)
		{
			CommandExecutionStatus status = CommandExecutionStatus.None;
			_stopwatch.Start();

			try
			{
				await _innerCommand.ExecuteAsync(token).ConfigureAwait(false);
				status = token.IsCancellationRequested ? CommandExecutionStatus.Canceled : CommandExecutionStatus.Completed;
			}
			catch (OperationCanceledException)
			{
				status = CommandExecutionStatus.Canceled;
				throw;
			}
			catch (Exception)
			{
				status = CommandExecutionStatus.Failed;
				throw;
			}
			finally
			{
				_stopwatch.Stop();
				_metrics.TimedOperation(typeof(T).Name, _stopwatch.Elapsed, status.ToString());
			}
		}
	}
}