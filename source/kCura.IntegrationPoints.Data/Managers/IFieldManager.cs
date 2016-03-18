using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Managers
{
	public interface IFieldManager
	{
		/// <summary>
		/// Retrieves the long text fields for an rdo
		/// </summary>
		/// <param name="rdoTypeId">The artifact id of the rdo's type</param>
		/// <returns>An array of ArtifactFieldDTO for the rdo</returns>
		Task<ArtifactFieldDTO[]> RetrieveLongTextFieldsAsync(int rdoTypeId);

		/// <summary>
		/// Retrieves fields for an rdo
		/// </summary>
		/// <param name="rdoTypeId">The artifact id of the rdo's type</param>
		/// <param name="fieldFieldsNames">The names of the fields to retrieve for the field artifact</param>
		/// <returns>An array of ArtifactDTO with populated fields for the given rdo type</returns>
		Task<ArtifactDTO[]> RetrieveFieldsAsync(int rdoTypeId, HashSet<string> fieldFieldsNames);
	}
}