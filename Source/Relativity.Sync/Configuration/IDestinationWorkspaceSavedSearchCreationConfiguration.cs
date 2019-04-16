using System.Threading.Tasks;

namespace Relativity.Sync.Configuration
{
	internal interface IDestinationWorkspaceSavedSearchCreationConfiguration : IConfiguration
	{
		int DestinationWorkspaceArtifactId { get; }

		string SourceJobTagName { get; }

		int SourceJobTagArtifactId { get; }

		bool CreateSavedSearchForTags { get; }

		bool IsSavedSearchArtifactIdSet { get; }

		Task SetSavedSearchInDestinationArtifactIdAsync(int artifactId);
	}
}