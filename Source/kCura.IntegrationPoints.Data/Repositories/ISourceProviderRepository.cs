using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	///     Repository responsible for Source Provider object
	/// </summary>
	public interface ISourceProviderRepository
	{
		/// <summary>
		///     Reads the Source Provider object instance
		/// </summary>
		/// <param name="artifactId">Artifact id of Source Provider instance</param>
		/// <returns>SourceProviderDTO object</returns>
		SourceProviderDTO Read(int artifactId);

		/// <summary>
		///     Gets the Source Provider artifact id given a guid identifier
		/// </summary>
		/// <param name="sourceProviderGuidIdentifier">Guid identifier of Source Provider type</param>
		/// <returns>Artifact id of the Source Provider</returns>
		int GetArtifactIdFromSourceProviderTypeGuidIdentifier(string sourceProviderGuidIdentifier);
	}
}