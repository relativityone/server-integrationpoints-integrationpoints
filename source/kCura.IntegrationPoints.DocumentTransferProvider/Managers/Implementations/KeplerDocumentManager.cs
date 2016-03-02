using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.DocumentTransferProvider.Models;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Managers.Implementations
{
	public class KeplerDocumentManager : IDocumentManager
	{
		private readonly IRDORepository _rdoRepository;

		public KeplerDocumentManager(IRDORepository rdoRepository)
		{
			_rdoRepository = rdoRepository;
		}

		public ArtifactDTO RetrieveDocument(int documentId, HashSet<int> fieldIds)
		{
			var documentsQuery = new Query()
			{
				Condition = $"'Artifact ID' == {documentId}",
				Fields = fieldIds.Select(x => x.ToString()).ToArray(),
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				TruncateTextFields = false
			};

			ObjectQueryResutSet resultSet = _rdoRepository.RetrieveAsync(documentsQuery, String.Empty).Result;

			if (resultSet != null && resultSet.Success)
			{
				ArtifactDTO document = resultSet.Data.DataResults.Select(
					x => new ArtifactDTO()
					{
						ArtifactId = x.ArtifactId,
						ArtifactTypeId = x.ArtifactTypeId,
						Fields =
							x.Fields.Select(
								y => new ArtifactFieldDTO() {ArtifactId = y.ArtifactId, FieldType = y.FieldType, Name = y.Name, Value = y.Value})
								.ToList()
					}).FirstOrDefault();

				return document;
			}

			throw new Exception(resultSet.Message);
		}

		public ArtifactDTO[] RetrieveDocuments(IEnumerable<int> documentIds, HashSet<int> fieldIds)
		{
			var documentsQuery = new Query()
			{
				Condition = $"'Artifact ID' in [{String.Join(",", documentIds)}]",
				Fields = fieldIds.Select(x => x.ToString()).ToArray(),
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				//Sorts = new[] { "'Artifact ID' ASC" },
				TruncateTextFields = false
			};

			ObjectQueryResutSet resultSet = _rdoRepository.RetrieveAsync(documentsQuery, String.Empty).Result;

			if (resultSet != null && resultSet.Success)
			{
				ArtifactDTO[] documents = resultSet.Data.DataResults.Select(
					x => new ArtifactDTO()
					{
						ArtifactId = x.ArtifactId,
						ArtifactTypeId = x.ArtifactTypeId,
						Fields = x.Fields.Select(y => new ArtifactFieldDTO()
						{
							ArtifactId = y.ArtifactId,
							FieldType = y.FieldType,
							Name = y.Name,
							Value = y.Value
						}).ToList()
					}).ToArray();

				return documents;
			}

			throw new Exception(resultSet.Message);
		}
	}
}