using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.Relativity.Client;
using Relativity.Services.ObjectQuery;
using Query = Relativity.Services.ObjectQuery.Query;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerDocumentRepository : KeplerServiceBase, IDocumentRepository
	{
		private readonly IObjectQueryManagerAdaptor _objectQueryManagerAdaptor;

		public KeplerDocumentRepository(IObjectQueryManagerAdaptor objectQueryManagerAdaptor) : base(objectQueryManagerAdaptor)
		{
			_objectQueryManagerAdaptor = objectQueryManagerAdaptor;
			_objectQueryManagerAdaptor.ArtifactTypeId = (int)ArtifactType.Document;
		}

		public int WorkspaceArtifactId
		{
			get { return _objectQueryManagerAdaptor.WorkspaceId; }
			set { _objectQueryManagerAdaptor.WorkspaceId = value; }
		}

		public async Task<ArtifactDTO> RetrieveDocumentAsync(int documentId, ICollection<int> fieldIds)
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

			ArtifactDTO[] documents = null;

			try
			{
				documents = await this.RetrieveAllArtifactsAsync(documentsQuery);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve document", e);
			}

			return documents.FirstOrDefault();
		}

		public async Task<ArtifactDTO[]> RetrieveDocumentsAsync(string docIdentifierField, ICollection<string> docIdentifierValues)
		{
			var documentsQuery = new Query()
			{
				Condition = $@"'{docIdentifierField}' in ['{String.Join("','", docIdentifierValues)}']",
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				TruncateTextFields = false
			};

			ArtifactDTO[] documents = null;

			try
			{
				documents = await this.RetrieveAllArtifactsAsync(documentsQuery);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve document", e);
			}

			return documents;
		}

		public async Task<ArtifactDTO[]> RetrieveDocumentsAsync(IEnumerable<int> documentIds, HashSet<int> fieldIds)
		{
			var documentsQuery = new Query()
			{
				Condition = $"'Artifact ID' in [{String.Join(",", documentIds)}]",
				Fields = fieldIds.Select(x => x.ToString()).ToArray(),
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				TruncateTextFields = false
			};

			ArtifactDTO[] documents = null;

			try
			{
				documents = await this.RetrieveAllArtifactsAsync(documentsQuery);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve documents", e);
			}

			return documents;
		}

		public async Task<int[]> RetrieveDocumentByIdentifierPrefixAsync(string documentIdentifierFieldName, string identifierPrefix)
		{
			var documentsQuery = new Query()
			{
				Condition = $"'{ documentIdentifierFieldName }' like '{ identifierPrefix }%'",
				Fields = new string[] { "ArtifactID" },
				IncludeIdWindow = false,
				SampleParameters = null,
				RelationalField = null,
				SearchProviderCondition = null,
				TruncateTextFields = false
			};

			int[] documentArtifactIds;

			try
			{
				var documents = await this.RetrieveAllArtifactsAsync(documentsQuery);
				documentArtifactIds = new int[documents.Length];

				for (int index = 0; index < documents.Length; index++)
				{
					documentArtifactIds[index] = documents[index].ArtifactId;
				}
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve documents", e);
			}

			return documentArtifactIds;
		}
	}
}