using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class DataDestinationFinalizationExecutionConstrains : IExecutionConstrains<IDataDestinationFinalizationConfiguration>
    {
        public Task<bool> CanExecuteAsync(IDataDestinationFinalizationConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(false);
        }
    }
}
