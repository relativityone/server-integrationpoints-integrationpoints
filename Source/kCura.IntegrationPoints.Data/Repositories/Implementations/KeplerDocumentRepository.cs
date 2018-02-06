using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerDocumentRepository : MarshalByRefObject, IDocumentRepository
	{
		private const int DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;
		private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;
		private IRelativityObjectManager _relativityObjectManager;
		private int _workspaceArtifactId;

		public KeplerDocumentRepository(IRelativityObjectManagerFactory relativityObjectManagerFactory, int workspaceArtifactId)
		{
			_relativityObjectManagerFactory = relativityObjectManagerFactory;
			WorkspaceArtifactId = workspaceArtifactId;
		}

		public int WorkspaceArtifactId
		{
			get { return _workspaceArtifactId; }
			set
			{
				if (_workspaceArtifactId != value)
				{
					_workspaceArtifactId = value;
					_relativityObjectManager = null;
				}
			}
		}

		public IRelativityObjectManager RelativityObjectManager
		{
			get
			{
				if (_relativityObjectManager == null)
				{
					_relativityObjectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(WorkspaceArtifactId);
				}
				return _relativityObjectManager;
			}
		}

		public async Task<int[]> RetrieveDocumentsAsync(string docIdentifierField, ICollection<string> docIdentifierValues)
		{
			var qr = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = DOCUMENT_ARTIFACT_TYPE_ID },
				Fields = new[] { new FieldRef { Name = "Artifact ID" } },
				Condition = $@"'{docIdentifierField}' in ['{string.Join("','", docIdentifierValues)}']"
			};

			try
			{
				List<RelativityObject> documents = await RelativityObjectManager.QueryAsync(qr).ConfigureAwait(false);
				return documents.Select(x => x.ArtifactID).ToArray();
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve document", e);
			}
		}

		public async Task<ArtifactDTO[]> RetrieveDocumentsAsync(IEnumerable<int> documentIds, HashSet<int> fieldIds)
		{
			var qr = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = DOCUMENT_ARTIFACT_TYPE_ID },
				Condition = $"'ArtifactID' in [{string.Join(",", documentIds)}]",
				Fields = fieldIds.Select(x => new FieldRef { ArtifactID = x }).ToArray()
			};

			try
			{
				List<RelativityObject> documents = await RelativityObjectManager.QueryAsync(qr).ConfigureAwait(false);
				var output = new ArtifactDTO[documents.Count];
				for (int i = 0; i < output.Length; i++)
				{
					output[i] = ConvertDocumentToArtifactDTO(documents[i]);
				}
				return output;
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve documents", e);
			}
		}

		private ArtifactDTO ConvertDocumentToArtifactDTO(RelativityObject document)
		{
			IEnumerable<ArtifactFieldDTO> fields = document.FieldValues.Select(ConvertFieldToArtifactFieldsDTO);
			var artifact = new ArtifactDTO(document.ArtifactID, DOCUMENT_ARTIFACT_TYPE_ID, document.Name, fields);
			return artifact;
		}

		private ArtifactFieldDTO ConvertFieldToArtifactFieldsDTO(FieldValuePair field)
		{
			return new ArtifactFieldDTO
			{
				ArtifactId = field.Field.ArtifactID,
				FieldType = field.Field.FieldType.ToString(),
				Name = field.Field.Name,
				Value = field.Value
			};
		}

		public async Task<int[]> RetrieveDocumentByIdentifierPrefixAsync(string documentIdentifierFieldName, string identifierPrefix)
		{
			var queryRequest = new QueryRequest
			{
				Condition = $"'{EscapeSingleQuote(documentIdentifierFieldName)}' like '{EscapeSingleQuote(identifierPrefix)}%'",
			};

			int[] documentArtifactIds;

			try
			{
				List<Document> documents = await RelativityObjectManager.QueryAsync<Document>(queryRequest, noFields: true).ConfigureAwait(false);
				documentArtifactIds = new int[documents.Count];

				for (int index = 0; index < documents.Count; index++)
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

		private string EscapeSingleQuote(string s)
		{
			return Regex.Replace(s, "'", "\\'");
		}
	}
}