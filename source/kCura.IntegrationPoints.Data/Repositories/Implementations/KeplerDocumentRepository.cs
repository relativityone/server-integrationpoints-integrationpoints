using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerDocumentRepository : IDocumentRepository
	{
		private readonly IObjectQueryManagerAdaptor _objectQueryManagerAdaptor;

		public KeplerDocumentRepository(IObjectQueryManagerAdaptor objectQueryManagerAdaptor)
		{
			_objectQueryManagerAdaptor = objectQueryManagerAdaptor;
		}

		public async Task<ArtifactDTO> RetrieveDocumentAsync(int documentId, ICollection<int> fieldIds)
		{
			var documentsQuery = new Query()
			{
				Condition = String.Format("'Artifact ID' == {0}", documentId),
				Fields = fieldIds.Select(x => x.ToString()).ToArray(),
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				TruncateTextFields = false
			};

			ObjectQueryResultSet resultSet = await _objectQueryManagerAdaptor.RetrieveAsync(documentsQuery, String.Empty);

			if (resultSet != null && resultSet.Success)
			{
				ArtifactDTO document = resultSet.Data.DataResults.Select(
					x => new ArtifactDTO(
						x.ArtifactId,
						x.ArtifactTypeId,
						x.Fields.Select(
							y => new ArtifactFieldDTO() {ArtifactId = y.ArtifactId, FieldType = y.FieldType, Name = y.Name, Value = y.Value}))
					).FirstOrDefault();

				return document;
			}

			throw new Exception(resultSet.Message);
		}

		public async Task<ArtifactDTO[]> RetrieveDocumentsAsync(IEnumerable<int> documentIds, HashSet<int> fieldIds)
		{
			var documentsQuery = new Query()
			{
				Condition = String.Format("'Artifact ID' in [{0}]", String.Join(",", documentIds)),
				Fields = fieldIds.Select(x => x.ToString()).ToArray(),
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				TruncateTextFields = false
			};

			ObjectQueryResultSet resultSet = await _objectQueryManagerAdaptor.RetrieveAsync(documentsQuery, String.Empty);

			if (resultSet != null && resultSet.Success)
			{
				ArtifactDTO[] documents = resultSet.Data.DataResults.Select(
					x => new ArtifactDTO(
						x.ArtifactId,
						x.ArtifactTypeId,
						x.Fields.Select(
							y => new ArtifactFieldDTO() {ArtifactId = y.ArtifactId, FieldType = y.FieldType, Name = y.Name, Value = y.Value}))
					).ToArray();

				return documents;
			}
			throw new Exception(resultSet.Message);
		}
	}
}