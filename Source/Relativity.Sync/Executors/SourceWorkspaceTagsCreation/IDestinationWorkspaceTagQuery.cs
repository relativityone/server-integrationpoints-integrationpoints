using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.SourceWorkspaceTagsCreation
{
	internal interface IDestinationWorkspaceTagQuery
	{
		Task<DestinationWorkspaceTag> QueryAsync(ISourceWorkspaceTagsCreationConfiguration configuration);
	}
}