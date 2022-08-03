using Relativity.API.Foundation;
using Relativity.Services.Interfaces.Field.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IFieldRepository
    {
        /// <summary>
        ///     Updates the field's filter type
        /// </summary>
        /// <param name="artifactViewFieldId">The artifact view field id of the field</param>
        /// <param name="filterType">The filter type to set</param>
        void UpdateFilterType(int artifactViewFieldId, string filterType);

        /// <summary>
        ///     Sets the overlay behavior for a field
        /// </summary>
        /// <param name="fieldArtifactId">The artifact id of the field</param>
        /// <param name="overlayBehavior">
        ///     The value for overlay behavior. <code>TRUE</code> for MERGE, <code>FALSE</code> for
        ///     Overlay
        /// </param>
        void SetOverlayBehavior(int fieldArtifactId, bool overlayBehavior);

        int CreateMultiObjectFieldOnDocument(string name, int associatedObjectTypeDescriptorId);

        int CreateObjectTypeField(BaseFieldRequest field);

        IField Read(int fieldArtifactId);

    }
}