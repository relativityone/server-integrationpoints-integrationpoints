using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class SourceWorkspaceTagsCreationStep : IExecutor<ISourceWorkspaceTagsCreationConfiguration>, IExecutionConstrains<ISourceWorkspaceTagsCreationConfiguration>
	{
		public SourceWorkspaceTagsCreationStep()
		{
		}

		public Task<bool> CanExecuteAsync(ISourceWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}

		public Task ExecuteAsync(ISourceWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			throw new System.NotImplementedException();
		}
	}
}