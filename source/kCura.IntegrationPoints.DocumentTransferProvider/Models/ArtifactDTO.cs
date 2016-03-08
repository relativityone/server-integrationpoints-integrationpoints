using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Models
{
	public class ArtifactDTO
	{
		private readonly IDictionary<int, ArtifactFieldDTO> _fieldDictionary;

		public ArtifactDTO(int artifactId, int artifactTypeId, IEnumerable<ArtifactFieldDTO> fields)
		{
			ArtifactId = artifactId;
			ArtifactTypeId = artifactTypeId;
			Fields = fields.ToList();
			_fieldDictionary = Fields.Where(x => x.ArtifactId > 0).ToDictionary(x => x.ArtifactId, y => y);
		}

		public int ArtifactId { get; private set; }
		public int ArtifactTypeId { get; private set; }
		public IList<ArtifactFieldDTO> Fields { get; private set; }

		/// <summary>
		/// Retrieves the ArtifactFieldDTO for the given identifier
		/// </summary>
		/// <param name="artifactId">The aritfact id of the field to find</param>
		/// <returns>The field with the given artifact id</returns>
		public ArtifactFieldDTO GetFieldForIdentifier(int artifactId)
		{
			if (_fieldDictionary == null)
			{
				return null;
			}

			ArtifactFieldDTO field = _fieldDictionary[artifactId];

			return field;
		}
	}
}
