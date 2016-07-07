using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Repository responsible for Integration Point object
	/// </summary>
	public interface IIntegrationPointRepository
	{
		/// <summary>
		/// Reads the Integration Point object instance
		/// </summary>
		/// <param name="artifactId">Artifact id of integration point instance</param>
		/// <returns>IntegrationPointDTO object</returns>
		IntegrationPointDTO Read(int artifactId);

		/// <summary>
		/// Reads the Integration Point object instances
		/// </summary>
		/// <param name="artifactIds">Artifact ids of integration point instances</param>
		/// <returns>IntegrationPointDTO objects</returns>
		List<IntegrationPointDTO> Read(IEnumerable<int> artifactIds);
	}
}
