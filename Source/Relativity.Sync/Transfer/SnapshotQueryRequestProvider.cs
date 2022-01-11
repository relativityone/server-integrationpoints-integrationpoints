﻿using System;
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
        private readonly ISourceServiceFactoryForAdmin _sourceServiceFactoryForAdmin;
        protected readonly ISyncLog _logger;

        private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

        public SnapshotQueryRequestProvider(ISnapshotQueryConfiguration configuration,
            IPipelineSelector pipelineSelector, IFieldManager fieldManager,
            ISourceServiceFactoryForAdmin sourceServiceFactoryForAdmin, ISyncLog logger)
        {
            _configuration = configuration;
            _pipelineSelector = pipelineSelector;
            _fieldManager = fieldManager;
            _sourceServiceFactoryForAdmin = sourceServiceFactoryForAdmin;
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
            string transferredRdoTypeNameInSourceWorkspace =
                await GetRdoTypeNameAsync(_configuration.SourceWorkspaceArtifactId, _configuration.RdoArtifactTypeId);
            
            token.ThrowIfCancellationRequested();
            
            string[] fieldsOfTheSameType =
                await GetSameTypeFieldNamesAsync(transferredRdoTypeNameInSourceWorkspace, _configuration.SourceWorkspaceArtifactId).ConfigureAwait(false);
            
            token.ThrowIfCancellationRequested();

            if (fieldsOfTheSameType.Any())
            {
                return new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        ArtifactTypeID = _configuration.RdoArtifactTypeId
                    },
                    Condition = GetConditionForFieldsWithSetValue(fieldsOfTheSameType)
                };
            }

            return null;
        }

        private string GetConditionForFieldsWithSetValue(string[] fieldNames)
        {
            return string.Join(" OR ", fieldNames.Select(name => $"('{name}' ISSET)"));
        }

        private async Task<string> GetRdoTypeNameAsync(int workspaceArtifactId, int rdoArtifactTypeId)
        {
            using (var objectManager =
                   await _sourceServiceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var query = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        ArtifactTypeID = (int)ArtifactType.ObjectType
                    },
                    Condition = $"'Artifact Type ID == {rdoArtifactTypeId}",
                    IncludeNameInQueryResult = true
                };

                QueryResult result =
                    await objectManager.QueryAsync(workspaceArtifactId, query, 0, 1).ConfigureAwait(false);

                if (result.Objects.Count != 1)
                {
                    _logger.LogError("Rdo with ArtifactTypeId {artifactTypeId} does not exist", rdoArtifactTypeId);
                    throw new SyncException($"Rdo with ArtifactTypeId {rdoArtifactTypeId} does not exist");
                }
                
                return result.Objects.Single().Name;
            }
        }

        private async Task<string[]> GetSameTypeFieldNamesAsync(string rdoTypeName, int workspaceId)
        {
            using (var objectManager =
                   await _sourceServiceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
                    Condition =
                        $"('Associative Object Type' == '{rdoTypeName}') AND ('Object Type' == '{rdoTypeName}')" +
                        $" AND (NOT ('Name' LIKE ['::']))" +
                        $" AND ('Field Type' IN ['Multiple Object', 'Single Object'])",
                    Fields = new[] { new FieldRef { Name = "Name" } }
                };

                QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceId, request, 0, Int32.MaxValue)
                    .ConfigureAwait(false);

                return result.Objects.SelectMany(x => x.Values.Select(v => v.ToString())).ToArray();
            }
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
                   await _sourceServiceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            using (IChoiceQueryManager choiceQueryManager = await _sourceServiceFactoryForAdmin
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