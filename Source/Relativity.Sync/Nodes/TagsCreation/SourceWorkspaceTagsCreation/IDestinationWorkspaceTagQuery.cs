using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes.TagsCreation.SourceWorkspaceTagsCreation
{
	internal interface IDestinationWorkspaceTagQuery
	{
		Task<DestinationWorkspaceTag> QueryAsync(ISourceWorkspaceTagsCreationConfiguration configuration);
	}
}