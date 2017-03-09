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
		/// Retrieves fields for rdo, display name and field type
		/// </summary>
		/// <param name="rdoTypeId"></param>
		/// <param name="displayName"></param>
		/// <param name="fieldType"></param>
		/// <param name="fieldNames"></param>
		/// <returns></returns>
		Task<ArtifactDTO[]> RetrieveFieldsAsync(int rdoTypeId, string displayName, string fieldType,
			HashSet<string> fieldNames);

		/// <summary>
		/// Retrieves fields for an rdo
		/// </summary>
		/// <param name="rdoTypeId">The artifact id of the rdo's type</param>
		/// <param name="fieldNames">The names of the fields to retrieve for the field artifact</param>
		/// <returns>An array of ArtifactDTO with populated fields for the given rdo type</returns>
		ArtifactDTO[] RetrieveFields(int rdoTypeId, HashSet<string> fieldNames);

		/// <summary>
		/// Retrieves field Artifact if for rdo, display name and field type
		/// </summary>
		/// <param name="rdoTypeId">The artifact id of the rdo's type</param>
		/// <param name="displayName">The Display Name of the rdo's</param>
		/// <param name="fieldType">The Field Type of the rdo's</param>
		/// <param name="fieldNames">The names of the fields to retrieve for the field artifact</param>
		/// <returns>Field artifact id</returns>
		ArtifactDTO RetrieveField(int rdoTypeId, string displayName, string fieldType, HashSet<string> fieldNames);

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

		/// <summary>
		/// Retrieves potential begin bates fields
		/// </summary>
		/// <returns>A list of Artifact fields DTO</returns>
		ArtifactFieldDTO[] RetrieveBeginBatesFields();

		/// <summary>
		/// Retrieves the artifact view field id for the given field
		/// </summary>
		/// <param name="fieldArtifactId">The artifact id of the field</param>
		/// <returns>The artifact view field id if found, <code>NULL</code> otherwise</returns>
		int? RetrieveArtifactViewFieldId(int fieldArtifactId);

		/// <summary>
		/// Updates the field's filter type
		/// </summary>
		/// <param name="artifactViewFieldId">The artifact view field id of the field</param>
		/// <param name="filterType">The filter type to set</param>
		void UpdateFilterType(int artifactViewFieldId, string filterType);

		/// <summary>
		/// Sets the overlay behavior for a field
		/// </summary>
		/// <param name="fieldArtifactId">The artifact id of the field</param>
		/// <param name="overlayBehavior">The value for overlay behavior. <code>TRUE</code> for MERGE, <code>FALSE</code> for Overlay</param>
		void SetOverlayBehavior(int fieldArtifactId, bool overlayBehavior);
	}
}