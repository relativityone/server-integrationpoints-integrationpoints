using System.Collections.Generic;
using Relativity.Sync.Storage;

namespace Relativity.Sync.SyncConfiguration.FieldsMapping
{
    /// <summary>
    /// Provides methods for configuring fields mapping.
    /// </summary>
    public interface IFieldsMappingBuilder
    {
        /// <summary>
        /// Gets the list of fields mapping.
        /// </summary>
        List<FieldMap> FieldsMapping { get; }

        /// <summary>
        /// Adds identifier to the fields mapping.
        /// </summary>
        IFieldsMappingBuilder WithIdentifier();

        /// <summary>
        /// Adds field to the mapping by field Artifact ID.
        /// </summary>
        /// <param name="sourceFieldId">Source field Artifact ID.</param>
        /// <param name="destinationFieldId">Destination field Artifact ID.</param>
        /// <returns></returns>
        IFieldsMappingBuilder WithField(int sourceFieldId, int destinationFieldId);

        /// <summary>
        /// Adds field to the mapping by field name.
        /// </summary>
        /// <param name="sourceFieldName">Source field name.</param>
        /// <param name="destinationFieldName">Destination field name.</param>
        IFieldsMappingBuilder WithField(string sourceFieldName, string destinationFieldName);
    }
}
