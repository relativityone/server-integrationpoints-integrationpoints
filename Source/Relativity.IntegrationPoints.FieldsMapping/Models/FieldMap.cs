using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Contracts.Models;

namespace Relativity.IntegrationPoints.FieldsMapping.Models
{
    /// <summary>
    /// Defines the mapping of fields in the data source to fields in a workspace.
    /// </summary>
    public class FieldMap
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
        /// Gets or sets the FieldMapTypeEnum, which indicates the type of mapping.
        /// </summary>
        public FieldMapTypeEnum FieldMapType { get; set; }
    }
}
