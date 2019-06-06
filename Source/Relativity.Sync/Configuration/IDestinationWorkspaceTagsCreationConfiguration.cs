using System.Threading.Tasks;

namespace Relativity.Sync.Configuration
{
	internal interface IDestinationWorkspaceTagsCreationConfiguration : IConfiguration
	{
		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceArtifactId { get; }

		int JobHistoryArtifactId { get; }

		bool IsSourceJobTagSet { get; }

		Task SetSourceJobTagAsync(int artifactId, string name);

		bool IsSourceWorkspaceTagSet { get; }

		Task SetSourceWorkspaceTagAsync(int artifactId, string name);
	}
}