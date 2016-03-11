using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public async Task<ArtifactFieldDTO[]> RetrieveLongTextFieldsAsync(int rdoTypeId)
		{
			const string longTextFieldName = "Long Text";

			var longTextFieldsQuery = new Query()
			{
				Condition = String.Format("'Object Type Artifact Type ID' == {0} AND 'Field Type' == '{1}'", rdoTypeId, longTextFieldName),
			};

			ObjectQueryResutSet result = await _rdoRepository.RetrieveAsync(longTextFieldsQuery, String.Empty);

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

		public async Task<ArtifactDTO[]> RetrieveFieldsAsync(int rdoTypeId, HashSet<string> fieldFieldsNames)
		{
			var fieldQuery = new Query()
			{
				Fields = fieldFieldsNames.ToArray(),
				Condition = String.Format("'Object Type Artifact Type ID' == {0}", rdoTypeId)
			};

			ObjectQueryResutSet result = await _rdoRepository.RetrieveAsync(fieldQuery, String.Empty);

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