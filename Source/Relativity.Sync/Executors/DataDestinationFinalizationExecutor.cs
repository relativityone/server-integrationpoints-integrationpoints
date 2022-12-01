using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    /// <summary>
    ///     Current implementation is empty.
    ///     It will be used to mark productions as produced in destination workspace when pushing productions will be
    ///     supported.
    /// </summary>
    internal sealed class DataDestinationFinalizationExecutor : IExecutor<IDataDestinationFinalizationConfiguration>
    {
        public Task<ExecutionResult> ExecuteAsync(IDataDestinationFinalizationConfiguration configuration, CompositeCancellationToken token)
        {
            return Task.FromResult(ExecutionResult.Success());
        }
    }
}
