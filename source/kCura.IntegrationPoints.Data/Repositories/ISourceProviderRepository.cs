using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Repository responsible for Source Provider object
	/// </summary>
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