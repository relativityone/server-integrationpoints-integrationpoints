using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.DocumentTransferProvider.Models;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Managers.Implementations
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
				Condition = $"'Object Type Artifact Type ID' == {rdoTypeId} AND 'Field Type' == '{longTextFieldName}'",
			};

			ObjectQueryResutSet result = _rdoRepository.RetrieveAsync(longTextFieldsQuery, String.Empty).Result;

			if (!result.Success)
			{
				var messages = result.Message;
				var e = messages;
				throw new Exception(e);
			}

			ArtifactFieldDTO[] fieldDtos = result.Data.DataResults.Select(x => new ArtifactFieldDTO()
			{
				ArtifactId = x.ArtifactId,
				FieldType = longTextFieldName,
				Name = x.TextIdentifier,
				Value = null
			}).ToArray();

			return fieldDtos;
		}

		public ArtifactDTO[] RetrieveFields(int rdoTypeId, HashSet<string> fieldFieldsNames)
		{
			var fieldQuery = new Query()
			{
				Fields = fieldFieldsNames.ToArray(),
				Condition = $"'Object Type Artifact Type ID' == {rdoTypeId}"
			};

			ObjectQueryResutSet result = _rdoRepository.RetrieveAsync(fieldQuery, String.Empty).Result;

			if (!result.Success)
			{
				var messages = result.Message;
				var e = messages;
				throw new Exception(e);
			}

			ArtifactDTO[] fieldArtifacts = result.Data.DataResults.Select(x =>
				new ArtifactDTO()
				{
					ArtifactId = x.ArtifactId,
					ArtifactTypeId = x.ArtifactTypeId,
					Fields = x.Fields.Select(y => new ArtifactFieldDTO()
					{
						Name = y.Name,
						ArtifactId = y.ArtifactId,
						FieldType = y.FieldType,
						Value = y.Value,
					}).ToArray()
				}).ToArray();

			return fieldArtifacts;
		}
	}
}