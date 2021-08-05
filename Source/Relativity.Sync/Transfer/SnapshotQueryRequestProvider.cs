using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.ChoiceQuery;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Transfer
{
	internal class SnapshotQueryRequestProvider : ISnapshotQueryRequestProvider
	{
		private readonly ISnapshotQueryConfiguration _configuration;
		private readonly IPipelineSelector _pipelineSelector;
		private readonly IFieldManager _fieldManager;
		private readonly ISyncServiceManager _servicesMgr;

		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

		public SnapshotQueryRequestProvider(ISnapshotQueryConfiguration configuration, IPipelineSelector pipelineSelector, IFieldManager fieldManager, ISyncServiceManager syncServiceManager)
		{
			_configuration = configuration;
			_pipelineSelector = pipelineSelector;
			_fieldManager = fieldManager;
			_servicesMgr = syncServiceManager;

		}

		public Task<QueryRequest> GetRequestForCurrentPipelineAsync(CancellationToken token)
		{
			return GetRequestForCurrentPipelineInternalAsync(false, token);
		}

		public Task<QueryRequest> GetRequestWithIdentifierOnlyForCurrentPipelineAsync(CancellationToken token)
		{
			return GetRequestForCurrentPipelineInternalAsync(true, token);
		}

		private async Task<QueryRequest> GetRequestForCurrentPipelineInternalAsync(bool withIdentifierOnly,
			CancellationToken token)
		{
			var pipeline = _pipelineSelector.GetPipeline();
			if (pipeline.IsDocumentPipeline())
			{
				IEnumerable<FieldInfoDto> fields = await GetDocumentFieldsAsync(withIdentifierOnly, token).ConfigureAwait(false);

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

			throw new SyncException("Unable to determine Sync flow type. Snapshot query request creation failed");
		}

		private async Task<IEnumerable<FieldInfoDto>> GetDocumentFieldsAsync(bool withIdentifierOnly, CancellationToken token)
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
			FieldInfoDto identifierField = await _fieldManager.GetObjectIdentifierFieldAsync(token).ConfigureAwait(false);

			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
				},
				Condition = $"{DocumentsInSavedSearch()} AND {DocumentsWithImages()}",
				Fields = new[]
				{
					new FieldRef { Name = identifierField.SourceFieldName }
				}
			};
			return queryRequest;
		}

		private async Task<QueryRequest> CreateImageRetryQueryRequestAsync(CancellationToken token)
		{
			FieldInfoDto identifierField = await _fieldManager.GetObjectIdentifierFieldAsync(token).ConfigureAwait(false);

			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
				},
				Condition = $"{DocumentsWithErrors()} AND {DocumentsInSavedSearch()} AND {DocumentsWithImages()}",
				Fields = new[]
				{
					new FieldRef { Name = identifierField.SourceFieldName }
				}
			};
			return queryRequest;
		}

        private async Task<int> GetGuidOfYesHoiceOnHasImagesAsync()
        {
            int choiceYesArtifactId;
            int fieldArtifactID;

            using (IObjectManager objectManager = _servicesMgr.CreateProxy<IObjectManager>(API.ExecutionIdentity.CurrentUser))
            {
                QueryResult result = await objectManager.QueryAsync(_configuration.workspaceId, new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Field },
                    Condition = "'Name' == 'Has Images'",
                }, 0, 1).ConfigureAwait(false);
				fieldArtifactID = result.Objects.ToArray().First().ArtifactID;
            }

            using (IChoiceQueryManager choiceQueryManager = _servicesMgr.CreateProxy<IChoiceQueryManager>(API.ExecutionIdentity.CurrentUser))
            {
                List<Services.ChoiceQuery.Choice> fieldChoicesList = await choiceQueryManager.QueryAsync(_configuration.workspaceId, fieldArtifactID);
                Services.ChoiceQuery.Choice yesChoice = fieldChoicesList.Find(choice => choice.Name == "Yes");
                choiceYesArtifactId = yesChoice.ArtifactID;
            }

            return choiceYesArtifactId;
        }

        private async Task<string> DocumentsWithImages()
		{
			int choiceArtifactId = await GetGuidOfYesHoiceOnHasImagesAsync();
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

		private string DocumentsWithOriginalImages(int choiceArtifactId)  => $"('Has Images' == CHOICE {choiceArtifactId})"; // "Has Images" == "Yes"

		private string DocumentsInSavedSearch() => $"('ArtifactId' IN SAVEDSEARCH {_configuration.DataSourceArtifactId})";

		private string DocumentsWithErrors() => $"(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{_configuration.JobHistoryToRetryId}]))";
	}
}
