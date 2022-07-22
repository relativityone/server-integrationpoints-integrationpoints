using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Utils;

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

        public Task<bool> CanExecuteAsync(CancellationToken token)
        {
            return MeasureExecutionTimeAsync(() => _innerCommand.CanExecuteAsync(token), "CanExecute", token);
        }

        public Task<ExecutionResult> ExecuteAsync(CompositeCancellationToken token)
        {
            return MeasureExecutionTimeAsync(() => _innerCommand.ExecuteAsync(token), "Execute", token.StopCancellationToken);
        }

        private async Task<TResult> MeasureExecutionTimeAsync<TResult>(Func<Task<TResult>> action, string actionName, CancellationToken token)
        {
            ExecutionStatus status = ExecutionStatus.None;
            _stopwatch.Start();

            try
            {
                TResult actionResult = await action().ConfigureAwait(false);
                status = token.IsCancellationRequested ? ExecutionStatus.Canceled : ExecutionStatus.Completed;
                return actionResult;
            }
            catch (OperationCanceledException)
            {
                status = ExecutionStatus.Canceled;
                throw;
            }
            catch (Exception)
            {
                status = ExecutionStatus.Failed;
                throw;
            }
            finally
            {
                _stopwatch.Stop();
                _metrics.Send(new CommandMetric($"{typeof(T).Name}.{actionName}")
                {
                    ExecutionStatus = status,
                    Duration = _stopwatch.Elapsed.TotalMilliseconds
                });
            }
        }
    }
}