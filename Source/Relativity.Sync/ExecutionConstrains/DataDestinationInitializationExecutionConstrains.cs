using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class DataDestinationInitializationExecutionConstrains : IExecutionConstrains<IDataDestinationInitializationConfiguration>
    {
        public Task<bool> CanExecuteAsync(IDataDestinationInitializationConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(false);
        }
    }
}