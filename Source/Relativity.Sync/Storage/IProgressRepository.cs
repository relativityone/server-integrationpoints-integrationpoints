using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	internal interface IProgressRepository
	{
		Task<IProgress> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, string name, int order, string status);
		Task<IProgress> GetAsync(int workspaceArtifactId, int artifactId);
		Task<IProgress> QueryAsync(int workspaceArtifactId, int syncConfigurationArtifactId, string name);
	}
}