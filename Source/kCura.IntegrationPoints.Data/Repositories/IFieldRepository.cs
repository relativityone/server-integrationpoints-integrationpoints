using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Responsible for handling Relativity fields
	/// </summary>
	public interface IFieldRepository
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
		/// <param name="fieldNames">The names of the fields to retrieve for the field artifact</param>
		/// <returns>An array of ArtifactDTO with populated fields for the given rdo type</returns>
		Task<ArtifactDTO[]> RetrieveFieldsAsync(int rdoTypeId, HashSet<string> fieldNames);

		/// <summary>
		/// Retrieves fields for an rdo
		/// </summary>
		/// <param name="rdoTypeId">The artifact id of the rdo's type</param>
		/// <param name="fieldNames">The names of the fields to retrieve for the field artifact</param>
		/// <returns>An array of ArtifactDTO with populated fields for the given rdo type</returns>
		ArtifactDTO[] RetrieveFields(int rdoTypeId, HashSet<string> fieldNames);

		/// <summary>
		/// Deletes the specified fields
		/// </summary>
		/// <param name="artifactIds">The artifact ids of the fields to delete</param>
		void Delete(IEnumerable<int> artifactIds);

		/// <summary>
		/// Retrieves the identifier field. NOTE : the returns ArtifactDTO contains name and 'is identifier' fields
		/// </summary>
		/// <param name="rdoTypeId"></param>
		/// <returns>the ArtifactDTO represents the identifier field of the object</returns>
		/// <remarks>the returns ArtifactDTO contains name and 'is identifier' fields</remarks>
		ArtifactDTO RetrieveTheIdentifierField(int rdoTypeId);

		/// <summary>
		/// Reads a given Field Dto
		/// </summary>
		/// <param name="dto">Field Dto to be read</param>
		/// <returns>A ResultSet of Field matching the provided Field Dto</returns>
		ResultSet<Field> Read(Field dto);
	}
}