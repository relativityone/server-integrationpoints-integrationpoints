using System;

namespace Relativity.Sync.Storage
{
    /// <summary>
    /// Retrieves fields from the data source that users can map in the Relativity UI.
    /// </summary>
    [Serializable]
    public sealed class FieldEntry
    {
        /// <summary>
        /// Gets or sets a user-friendly name for display in the Relativity UI.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a field identifier used when mapping data source fields to workspace fields.
        /// </summary>
        public int FieldIdentifier { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the field contains a unique identifier for the data.
        /// </summary>
        public bool IsIdentifier { get; set; }
    }
}
