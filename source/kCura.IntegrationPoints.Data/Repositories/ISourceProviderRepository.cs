using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface ISourceProviderRepository
	{
		/// <summary>
		/// Reads the Source Provider object instance
		/// </summary>
		/// <param name="artifactId">Artifact id of Source Provider instance</param>
		/// <returns>SourceProviderDTO object</returns>
		SourceProviderDTO Read(int artifactId);
	}
}