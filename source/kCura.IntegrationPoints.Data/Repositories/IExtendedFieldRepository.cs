using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IExtendedFieldRepository
	{
		/// <summary>
		/// Retrieves the potential begin bates fields
		/// </summary>
		/// <returns>An array of ArtifactFieldDTOs</returns>
		ArtifactFieldDTO[] RetrieveBeginBatesFields();

		/// <summary>
		/// Retrieves the artifact ID of a field.
		/// </summary>
		/// <param name="displayName">The display name of the field.</param>
		/// <param name="fieldArtifactTypeId">The object type artifact ID the field is associated with.</param>
		/// <param name="fieldTypeId">The field type ID.</param>
		/// <returns>The artifact ID for the field, <code>NULL</code> if not found.</returns>
		int? RetrieveField(string displayName, int fieldArtifactTypeId, int fieldTypeId);

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
		/// <param name="value">The value for overlay behavior. <code>TRUE</code> for MERGE, <code>FALSE</code> for Overlay</param>
		void SetOverlayBehavior(int fieldArtifactId, bool value);
	}
}