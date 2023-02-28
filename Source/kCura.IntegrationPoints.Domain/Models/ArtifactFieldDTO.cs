using System;
using Relativity;

namespace kCura.IntegrationPoints.Domain.Models
{
    /// <summary>
    /// A data transfer object class used representing a Relativity field.
    /// </summary>
    [Serializable]
    public class ArtifactFieldDTO
    {
        /// <summary>
        /// Gets or sets the artifact id of the field.
        /// </summary>
        public int ArtifactId { get; set; }

        /// <summary>
        /// Gets or sets the type of the field.
        /// </summary>
        public FieldTypeHelper.FieldType FieldType { get; set; }

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the field.
        /// </summary>
        public virtual object Value { get; set; }
    }
}
