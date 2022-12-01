using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class PermissionCheckExecutionConstrains : IExecutionConstrains<IPermissionsCheckConfiguration>
    {
        public Task<bool> CanExecuteAsync(IPermissionsCheckConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(true);
        }
    }
}
