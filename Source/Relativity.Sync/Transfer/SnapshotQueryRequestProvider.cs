using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

		private const string _DOCUMENTS_WITH_PRODUCED_IMAGES = "('Production::Image Count' > 0)";
		private const string _DOCUMENTS_WITH_ORIGINAL_IMAGES = "('Has Images' == CHOICE 1034243)";

		public SnapshotQueryRequestProvider(ISnapshotQueryConfiguration configuration, IPipelineSelector pipelineSelector, IFieldManager fieldManager)
		{
			_configuration = configuration;
			_pipelineSelector = pipelineSelector;
			_fieldManager = fieldManager;
		}

		public async Task<QueryRequest> GetRequestForCurrentPipelineAsync(CancellationToken token)
		{
			var pipeline = _pipelineSelector.GetPipeline();
			if (pipeline.IsDocumentPipeline())
			{
				return pipeline.IsRetryPipeline()
					? await CreateDocumentRetryQueryRequestAsync(token).ConfigureAwait(false)
					: await CreateDocumentQueryRequestAsync(token).ConfigureAwait(false);
			}
			else if (pipeline.IsImagePipeline())
			{
				return pipeline.IsRetryPipeline()
					? await CreateImageRetryQueryRequestAsync(token).ConfigureAwait(false)
					: await CreateImageQueryRequestAsync(token).ConfigureAwait(false);
			}
			else
			{
				//TODO
				throw new Exception();
			}
		}

		private async Task<QueryRequest> CreateDocumentQueryRequestAsync(CancellationToken token)
		{
			IEnumerable<FieldInfoDto> documentFields = await _fieldManager.GetDocumentTypeFieldsAsync(token).ConfigureAwait(false);

			return new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
				},
				Condition = DocumentsInSavedSearch(),
				Fields = documentFields.Select(f => new FieldRef { Name = f.SourceFieldName }).ToList()
			};
		}

		private async Task<QueryRequest> CreateDocumentRetryQueryRequestAsync(CancellationToken token)
		{
			IEnumerable<FieldInfoDto> documentFields = await _fieldManager.GetDocumentTypeFieldsAsync(token).ConfigureAwait(false);

			return new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
				},
				Condition = $"{DocumentsWithErrors()} AND {DocumentsInSavedSearch()}",
				Fields = documentFields.Select(f => new FieldRef { Name = f.SourceFieldName }).ToList()
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

		private string DocumentsWithImages()
		{
			if (_configuration.ProductionImagePrecedence.Any())
			{
				return _configuration.IncludeOriginalImageIfNotFoundInProductions
					? $"({_DOCUMENTS_WITH_PRODUCED_IMAGES} OR {_DOCUMENTS_WITH_ORIGINAL_IMAGES})"
					: _DOCUMENTS_WITH_PRODUCED_IMAGES;
			}

			return _DOCUMENTS_WITH_ORIGINAL_IMAGES;
		}

		private string DocumentsInSavedSearch() => $"('ArtifactId' IN SAVEDSEARCH {_configuration.DataSourceArtifactId})";

		private string DocumentsWithErrors() => $"(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{_configuration.JobHistoryToRetryId}]))";
	}
}
