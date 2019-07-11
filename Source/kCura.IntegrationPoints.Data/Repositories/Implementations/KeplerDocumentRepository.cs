using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerDocumentRepository : MarshalByRefObject, IDocumentRepository
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;
		private readonly IRelativityObjectManager _relativityObjectManager;

		public KeplerDocumentRepository(IRelativityObjectManager relativityObjectManager)
		{
			_relativityObjectManager = relativityObjectManager;
		}

		public async Task<int[]> RetrieveDocumentsAsync(string docIdentifierField, ICollection<string> docIdentifierValues)
		{
			var qr = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID },
				Fields = new[] { new FieldRef { Name = "Artifact ID" } },
				Condition = $@"'{docIdentifierField}' in ['{string.Join("','", docIdentifierValues)}']"
			};

			List<RelativityObject> documents = await _relativityObjectManager.QueryAsync(qr).ConfigureAwait(false);
			return documents.Select(x => x.ArtifactID).ToArray();
		}

		public async Task<ArtifactDTO[]> RetrieveDocumentsAsync(IEnumerable<int> documentIds, HashSet<int> fieldIds)
		{
			var qr = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID },
				Condition = $"'ArtifactID' in [{string.Join(",", documentIds)}]",
				Fields = fieldIds.Select(x => new FieldRef { ArtifactID = x }).ToArray()
			};

			List<RelativityObject> documents = await _relativityObjectManager.QueryAsync(qr).ConfigureAwait(false);
			return documents.Select(ConvertDocumentToArtifactDTO).ToArray();
		}

		public async Task<int[]> RetrieveDocumentByIdentifierPrefixAsync(string documentIdentifierFieldName, string identifierPrefix)
		{
			var queryRequest = new QueryRequest
			{
				Condition = $"'{EscapeSingleQuote(documentIdentifierFieldName)}' like '{EscapeSingleQuote(identifierPrefix)}%'",
			};

			List<Document> documents = await _relativityObjectManager
				.QueryAsync<Document>(queryRequest, noFields: true)
				.ConfigureAwait(false);
			return documents.Select(x => x.ArtifactId).ToArray();
		}

		public Task<bool> MassUpdateAsync(IEnumerable<int> artifactIDsToUpdate, IEnumerable<FieldUpdateRequestDto> fieldsToUpdate)
		{
			IEnumerable<FieldRefValuePair> convertedFieldstoUpdate = fieldsToUpdate.Select(x => x.ToFieldRefValuePair());
			return _relativityObjectManager.MassUpdateAsync(artifactIDsToUpdate, convertedFieldstoUpdate, FieldUpdateBehavior.Merge);
		}

		private ArtifactDTO ConvertDocumentToArtifactDTO(RelativityObject document)
		{
			IEnumerable<ArtifactFieldDTO> fields = document.FieldValues.Select(ConvertFieldToArtifactFieldsDTO);
			var artifact = new ArtifactDTO(document.ArtifactID, _DOCUMENT_ARTIFACT_TYPE_ID, document.Name, fields);
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

		private string EscapeSingleQuote(string s)
		{
			return Regex.Replace(s, "'", "\\'");
		}
	}
}