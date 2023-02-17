using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.Services.DataContracts.DTOs.Results;
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

        public async Task<ArtifactDTO[]> RetrieveDocumentsAsync(IEnumerable<int> documentIDs, HashSet<string> fieldNames)
        {
            var qr = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID },
                Condition = $"'ArtifactID' in [{string.Join(",", documentIDs)}]",
                Fields = fieldNames.Select(fieldName => new FieldRef { Name = fieldName }).ToArray()
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

        public Task<ExportInitializationResultsDto> InitializeSearchExportAsync(
            int searchArtifactID,
            int[] artifactFieldIDs,
            int startAtRecord)
        {
            QueryRequest request = CreateQueryRequestForSearchExport(searchArtifactID, artifactFieldIDs);
            return InitializeExportAsync(request, startAtRecord);
        }

        public Task<ExportInitializationResultsDto> InitializeProductionExportAsync(
            int productionArtifactID,
            int[] artifactFieldIDs,
            int startAtRecord)
        {
            QueryRequest request = CreateQueryRequestForProductionExport(productionArtifactID, artifactFieldIDs);
            return InitializeExportAsync(request, startAtRecord);
        }

        public async Task<IList<RelativityObjectSlimDto>> RetrieveResultsBlockFromExportAsync(
            ExportInitializationResultsDto initializationResults,
            int resultsBlockSize,
            int exportIndexID)
        {
            RelativityObjectSlim[] objects = await _relativityObjectManager
                .RetrieveResultsBlockFromExportAsync(initializationResults.RunID, resultsBlockSize, exportIndexID)
                .ConfigureAwait(false);

            IList<RelativityObjectSlimDto> objectDtos = objects
                .Select(objectSlim => objectSlim.ToRelativityObjectSlimDto(initializationResults.FieldNames))
                .ToList();

            return objectDtos;
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
                FieldType = (FieldTypeHelper.FieldType) field.Field.FieldType,
                Name = field.Field.Name,
                Value = field.Value
            };
        }

        private string EscapeSingleQuote(string s)
        {
            return Regex.Replace(s, "'", "\\'");
        }

        private static QueryRequest CreateQueryRequestForSearchExport(int searchArtifactID, int[] artifactFieldIDs)
        {
            IList<FieldRef> fields = GetFieldsListFromArtifactIDs(artifactFieldIDs);
            QueryRequest request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef {ArtifactTypeID = (int) ArtifactType.Document },
                Fields = fields,
                Condition = $"'ArtifactId' IN SAVEDSEARCH {searchArtifactID}"
            };
            return request;
        }

        private QueryRequest CreateQueryRequestForProductionExport(int productionArtifactID, int[] artifactFieldIDs)
        {
            IList<FieldRef> fields = GetFieldsListFromArtifactIDs(artifactFieldIDs);
            QueryRequest request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef {ArtifactTypeID = (int) ArtifactType.Document },
                Fields = fields,
                Condition = $"(('Production' SUBQUERY ((('Production::ProductionSet' == OBJECT {productionArtifactID})))))"
            };
            return request;
        }

        private static IList<FieldRef> GetFieldsListFromArtifactIDs(int[] artifactFieldIDs)
        {
            List<FieldRef> fields = artifactFieldIDs
                .Select(artifactFieldID => new FieldRef { ArtifactID = artifactFieldID })
                .ToList();
            return fields;
        }

        private async Task<ExportInitializationResultsDto> InitializeExportAsync(QueryRequest request, int startAtRecord)
        {
            ExportInitializationResults results = await _relativityObjectManager.InitializeExportAsync(request, startAtRecord)
                .ConfigureAwait(false);

            return new ExportInitializationResultsDto(
                results.RunID,
                results.RecordCount,
                results.FieldData.Select(x => x.Name).ToArray());
        }
    }
}
