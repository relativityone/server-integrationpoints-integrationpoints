using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class SourceWorkspaceObjectTypesCreationExecutor : IExecutor<ISourceWorkspaceObjectTypesCreationConfiguration>
	{
		public Task<ExecutionResult> ExecuteAsync(ISourceWorkspaceObjectTypesCreationConfiguration configuration, CompositeCancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}