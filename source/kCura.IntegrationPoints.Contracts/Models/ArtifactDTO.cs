﻿using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Contracts.Models
{
	/// <summary>
	/// A data transfer object class representing a Relativity Artifact.
	/// </summary>
	public class ArtifactDTO
	{
		private readonly IDictionary<int, ArtifactFieldDTO> _fieldDictionary;

		/// <summary>
		/// Public constructor for the ArtifactDTO class.
		/// </summary>
		/// <param name="artifactId">The artifact id of the object.</param>
		/// <param name="artifactTypeId">The artifact type id of the object.</param>
		/// <param name="textIdentifier">The text identifider for the object.</param>
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
		public int ArtifactId { get; private set; }

		/// <summary>
		/// The artifact type id of the object.
		/// </summary>
		public int ArtifactTypeId { get; private set; }

		/// <summary>
		/// The text identifier of the object.
		/// </summary>
		public string TextIdentifier { get; private set; }

		/// <summary>
		/// The fields for the object.
		/// </summary>
		public IList<ArtifactFieldDTO> Fields { get; private set; }

		/// <summary>
		/// Retrieves the ArtifactFieldDTO for the given identifier.
		/// </summary>
		/// <param name="artifactId">The artifact id of the field to find.</param>
		/// <returns>The field with the given artifact id.</returns>
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
