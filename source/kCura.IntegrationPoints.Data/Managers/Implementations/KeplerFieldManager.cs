using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data.Managers.Implementations
{
	public class KeplerFieldManager : IFieldManager
	{
		private readonly IRDORepository _rdoRepository;

		public KeplerFieldManager(IRDORepository rdoRepository)
		{
			_rdoRepository = rdoRepository;
		}

		public ArtifactFieldDTO[] RetrieveLongTextFields(int rdoTypeId)
		{
			const string longTextFieldName = "Long Text";

			var longTextFieldsQuery = new Query()
			{
				Condition = String.Format("'Object Type Artifact Type ID' == {0} AND 'Field Type' == '{1}'", rdoTypeId, longTextFieldName),
			};

			ObjectQueryResutSet result = _rdoRepository.RetrieveAsync(longTextFieldsQuery, String.Empty).Result;

			if (!result.Success)
			{
				throw new Exception(result.Message);
			}

			ArtifactFieldDTO[] fieldDtos = result.Data.DataResults.Select(x => new ArtifactFieldDTO()
			{
				ArtifactId = x.ArtifactId,
				FieldType = longTextFieldName,
				Name = x.TextIdentifier,
				Value = null // Field RDO's don't have values...setting this to NULL to be explicit
			}).ToArray();

			return fieldDtos;
		}

		public ArtifactDTO[] RetrieveFields(int rdoTypeId, HashSet<string> fieldFieldsNames)
		{
			var fieldQuery = new Query()
			{
				Fields = fieldFieldsNames.ToArray(),
				Condition = String.Format("'Object Type Artifact Type ID' == {0}", rdoTypeId)
			};

			ObjectQueryResutSet result = _rdoRepository.RetrieveAsync(fieldQuery, String.Empty).Result;

			if (!result.Success)
			{
				throw new Exception(result.Message);
			}

			ArtifactDTO[] fieldArtifacts = result.Data.DataResults.Select(x =>
				new ArtifactDTO(
					x.ArtifactId,
					x.ArtifactTypeId,
					x.Fields.Select(
						y => new ArtifactFieldDTO() { ArtifactId = y.ArtifactId, FieldType = y.FieldType, Name = y.Name, Value = y.Value }))
			).ToArray();

			return fieldArtifacts;
		}
	}
}