using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.ChoiceQuery;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Transfer
{
    internal class SnapshotQueryRequestProvider : ISnapshotQueryRequestProvider
    {
        private readonly ISnapshotQueryConfiguration _configuration;
        private readonly IPipelineSelector _pipelineSelector;
        private readonly IFieldManager _fieldManager;
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        protected readonly ISyncLog _logger;

        private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

        public SnapshotQueryRequestProvider(ISnapshotQueryConfiguration configuration,
            IPipelineSelector pipelineSelector, IFieldManager fieldManager,
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin, ISyncLog logger)
        {
            _configuration = configuration;
            _pipelineSelector = pipelineSelector;
            _fieldManager = fieldManager;
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _logger = logger;
        }

        public Task<QueryRequest> GetRequestForCurrentPipelineAsync(CancellationToken token)
        {
            return GetRequestForCurrentPipelineInternalAsync(false, token);
        }

        public Task<QueryRequest> GetRequestWithIdentifierOnlyForCurrentPipelineAsync(CancellationToken token)
        {
            return GetRequestForCurrentPipelineInternalAsync(true, token);
        }

        /// <inheritdoc/>
        public async Task<QueryRequest> GetRequestForLinkingNonDocumentObjectsAsync(CancellationToken token)
        {
            IReadOnlyList<FieldInfoDto> mappedFields = await _fieldManager.GetMappedFieldsNonDocumentForLinksAsync(token).ConfigureAwait(false);
            _logger.LogInformation("LinkingNonDocuments: Mapped fields count {mappedFieldsCount}", mappedFields.Count);
            
            // GetMappedFieldsNonDocumentForLinksAsync always return at least Identifier field
            if (mappedFields.Any(x => !x.IsIdentifier))
            {
                return new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        ArtifactTypeID = _configuration.RdoArtifactTypeId
                    },
                    Condition = GetConditionForFieldsWithSetValue(mappedFields.Where(x => x.IsIdentifier == false).Select(x => x.SourceFieldName)),
                    Fields = mappedFields.Select(f => new FieldRef { Name = f.SourceFieldName }).ToList()
                };
            }

            return null;
        }

        private string GetConditionForFieldsWithSetValue(IEnumerable<string> fieldNames)
        {
            var fieldNameCondition = string.Join(" OR ", fieldNames.Select(name => $"('{name}' ISSET)"));
            var viewCondition = $"('ArtifactId' IN VIEW {_configuration.DataSourceArtifactId})";
            return $"( {fieldNameCondition} ) AND {viewCondition}";
        }
        
        private async Task<QueryRequest> GetRequestForCurrentPipelineInternalAsync(bool withIdentifierOnly,
            CancellationToken token)
        {
            var pipeline = _pipelineSelector.GetPipeline();
            if (pipeline.IsDocumentPipeline())
            {
                IEnumerable<FieldInfoDto> fields =
                    await GetDocumentFieldsAsync(withIdentifierOnly, token).ConfigureAwait(false);

                return pipeline.IsRetryPipeline()
                    ? CreateDocumentRetryQueryRequest(fields)
                    : CreateDocumentQueryRequest(fields);
            }

            if (pipeline.IsImagePipeline())
            {
                return pipeline.IsRetryPipeline()
                    ? await CreateImageRetryQueryRequestAsync(token).ConfigureAwait(false)
                    : await CreateImageQueryRequestAsync(token).ConfigureAwait(false);
            }

            if (pipeline.IsNonDocumentPipeline())
            {
                IList<FieldInfoDto> fields = await _fieldManager.GetMappedFieldsAsync(token).ConfigureAwait(false);
                
                return CreateNonDocumentQueryRequest(fields);
            }
            
            throw new SyncException("Unable to determine Sync flow type. Snapshot query request creation failed");
        }

        private QueryRequest CreateNonDocumentQueryRequest(IEnumerable<FieldInfoDto> fields)
        {
            return new QueryRequest()
            {
                ObjectType = new ObjectTypeRef()
                {
                    ArtifactTypeID = _configuration.RdoArtifactTypeId
                },
                Condition = $"('ArtifactId' IN VIEW {_configuration.DataSourceArtifactId})",
                Fields = fields.Select(f => new FieldRef { Name = f.SourceFieldName }).ToList(),
                IncludeNameInQueryResult = true
            };
        }

        private async Task<IEnumerable<FieldInfoDto>> GetDocumentFieldsAsync(bool withIdentifierOnly,
            CancellationToken token)
        {
            return withIdentifierOnly
                ? new[] { await _fieldManager.GetObjectIdentifierFieldAsync(token).ConfigureAwait(false) }
                : await _fieldManager.GetDocumentTypeFieldsAsync(token).ConfigureAwait(false);
        }

        private QueryRequest CreateDocumentQueryRequest(IEnumerable<FieldInfoDto> fields)
        {
            return new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
                },
                Condition = DocumentsInSavedSearch(),
                Fields = fields.Select(f => new FieldRef { Name = f.SourceFieldName }).ToList()
            };
        }

        private QueryRequest CreateDocumentRetryQueryRequest(IEnumerable<FieldInfoDto> fields)
        {
            return new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
                },
                Condition = $"{DocumentsWithErrors()} AND {DocumentsInSavedSearch()}",
                Fields = fields.Select(f => new FieldRef { Name = f.SourceFieldName }).ToList()
            };
        }

        private async Task<QueryRequest> CreateImageQueryRequestAsync(CancellationToken token)
        {
            FieldInfoDto identifierField =
                await _fieldManager.GetObjectIdentifierFieldAsync(token).ConfigureAwait(false);

            QueryRequest queryRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
                },
                Condition = $"{DocumentsInSavedSearch()} AND {await DocumentsWithImages()}",
                Fields = new[]
                {
                    new FieldRef { Name = identifierField.SourceFieldName }
                }
            };
            return queryRequest;
        }

        private async Task<QueryRequest> CreateImageRetryQueryRequestAsync(CancellationToken token)
        {
            FieldInfoDto identifierField =
                await _fieldManager.GetObjectIdentifierFieldAsync(token).ConfigureAwait(false);

            QueryRequest queryRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
                },
                Condition = $"{DocumentsWithErrors()} AND {DocumentsInSavedSearch()} AND {await DocumentsWithImages()}",
                Fields = new[]
                {
                    new FieldRef { Name = identifierField.SourceFieldName }
                }
            };
            return queryRequest;
        }

        private async Task<int> GetGuidOfYesChoiceOnHasImagesAsync()
        {
            int choiceYesArtifactId;
            int fieldArtifactID;

            using (IObjectManager objectManager =
                   await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            using (IChoiceQueryManager choiceQueryManager = await _serviceFactoryForAdmin
                       .CreateProxyAsync<IChoiceQueryManager>().ConfigureAwait(false))
            {
                QueryResult result = await objectManager.QueryAsync(_configuration.SourceWorkspaceArtifactId,
                    new QueryRequest()
                    {
                        ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Field },
                        Condition = "'Name' == 'Has Images'",
                    }, 0, 1).ConfigureAwait(false);
                fieldArtifactID = result.Objects.ToArray().First().ArtifactID;

                List<Services.ChoiceQuery.Choice> fieldChoicesList =
                    await choiceQueryManager.QueryAsync(_configuration.SourceWorkspaceArtifactId, fieldArtifactID);
                Services.ChoiceQuery.Choice yesChoice = fieldChoicesList.FirstOrDefault(choice => choice.Name == "Yes");

                if (yesChoice == null)
                {
                    _logger.LogError(
                        "Unable to find choice with \"Yes\" name for \"Has Images\" field - this system field is in invalid state");
                    throw new SyncException(
                        "Unable to find choice with \"Yes\" name for \"Has Images\" field - this system field is in invalid state");
                }

                choiceYesArtifactId = yesChoice.ArtifactID;
            }

            return choiceYesArtifactId;
        }

        private async Task<string> DocumentsWithImages()
        {
            int choiceArtifactId = await GetGuidOfYesChoiceOnHasImagesAsync();
            string documentsWithOriginalImages = DocumentsWithOriginalImages(choiceArtifactId);
            if (_configuration.ProductionImagePrecedence.Any())
            {
                return _configuration.IncludeOriginalImageIfNotFoundInProductions
                    ? $"({DocumentsWithProducedImages} OR {documentsWithOriginalImages})"
                    : DocumentsWithProducedImages;
            }

            return documentsWithOriginalImages;
        }

        private static string DocumentsWithProducedImages => "('Production::Image Count' > 0)";

        private string DocumentsWithOriginalImages(int yesChoiceArtifactId) =>
            $"('Has Images' == CHOICE {yesChoiceArtifactId})"; // "Has Images" == "Yes"

        private string DocumentsInSavedSearch() =>
            $"('ArtifactId' IN SAVEDSEARCH {_configuration.DataSourceArtifactId})";

        private string DocumentsWithErrors() =>
            $"(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{_configuration.JobHistoryToRetryId}]))";
    }
}