using System;

namespace Relativity.Sync.Storage
{
    /// <summary>
    /// Defines the mapping of fields in the data source to fields in a workspace.
    /// </summary>
    [Serializable]
    public sealed class FieldMap
    {
        /// <summary>
        /// Gets or sets the field in the source where the data is stored.
        /// </summary>
        public FieldEntry SourceField { get; set; }

        /// <summary>
        /// Gets or sets the field in the workspace used to store data imported from a data source.
        /// </summary>
        public FieldEntry DestinationField { get; set; }

        /// <summary>
        /// Gets or sets the FieldMapType, which indicates the type of mapping.
        /// </summary>
        public FieldMapType FieldMapType { get; set; }

        /// <summary>
        /// Returns string representation of the FieldMap
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FieldMapType + ": " + (SourceField?.FieldIdentifier ?? -1) + "<-->" + (DestinationField?.FieldIdentifier ?? -1);
        }
    }
}
