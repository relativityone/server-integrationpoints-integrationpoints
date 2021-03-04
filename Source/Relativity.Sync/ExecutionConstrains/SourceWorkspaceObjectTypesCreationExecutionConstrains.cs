using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class SourceWorkspaceObjectTypesCreationExecutionConstrains : IExecutionConstrains<ISourceWorkspaceObjectTypesCreationConfiguration>
	{
		public Task<bool> CanExecuteAsync(ISourceWorkspaceObjectTypesCreationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}
	}
}