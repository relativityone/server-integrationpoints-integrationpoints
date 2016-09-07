using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Repository responsible for Destination Provider object
	/// </summary>
	public interface IDestinationProviderRepository
	{
		/// <summary>
		/// Reads the Destination Provider object instance
		/// </summary>
		/// <param name="artifactId">Artifact id of Destination Provider instance</param>
		/// <returns>DestinationProviderDTO object</returns>

		DestinationProviderDTO Read(int artifactId);
	}
}