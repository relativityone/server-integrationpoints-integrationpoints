using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Domain.Models
{
    /// <summary>
    /// A data transfer object class representing a Relativity Artifact.
    /// </summary>
    [Serializable]
    public class ArtifactDTO
    {
        private readonly IDictionary<int, ArtifactFieldDTO> _fieldDictionary;

        /// <summary>
        /// Public constructor for the ArtifactDTO class.
        /// </summary>
        /// <param name="artifactId">The artifact id of the object.</param>
        /// <param name="artifactTypeId">The artifact type id of the object.</param>
        /// <param name="textIdentifier">The text identifier for the object.</param>
        /// <param name="fields">The fields of the artifact.</param>
        public ArtifactDTO(
            int artifactId,
            int artifactTypeId,
            string textIdentifier,
            IEnumerable<ArtifactFieldDTO> fields)
        {
            ArtifactId = artifactId;
            ArtifactTypeId = artifactTypeId;
            TextIdentifier = textIdentifier;

            Fields = fields.ToList();
            _fieldDictionary = Fields.Where(x => x.ArtifactId > 0).ToDictionary(x => x.ArtifactId, y => y);
        }

        /// <summary>
        /// The artifact id of the object.
        /// </summary>
        public int ArtifactId { get; }

        /// <summary>
        /// The artifact type id of the object.
        /// </summary>
        public int ArtifactTypeId { get; }

        /// <summary>
        /// The text identifier of the object.
        /// </summary>
        public string TextIdentifier { get; }

        /// <summary>
        /// The fields for the object.
        /// </summary>
        public IList<ArtifactFieldDTO> Fields { get; }

        /// <summary>
        /// Retrieves the ArtifactFieldDTO for the given identifier.
        /// </summary>
        /// <param name="artifactId">The artifact id of the field to find.</param>
        /// <returns>The field with the given artifact id.</returns>
        public ArtifactFieldDTO GetFieldForIdentifier(int artifactId)
        {
            ArtifactFieldDTO field = _fieldDictionary?[artifactId];
            return field;
        }

        public ArtifactFieldDTO GetFieldByName(string name)
        {
            ArtifactFieldDTO field = Fields.FirstOrDefault(_ => _.Name == name);
            return field;
        }
    }
}
