using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal abstract class ImageDataSourceSnapshotExecutorBase<T> where T : IImageDataSourceSnapshotConfiguration
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;
		private const string _DOCUMENTS_WITH_PRODUCED_IMAGES = "('Production::Image Count' > 0)";
		private const string _DOCUMENTS_WITH_ORIGINAL_IMAGES = "('Has Images' == CHOICE 1034243)";

		private readonly IImageFileRepository _imageFileRepository;
		private readonly IFieldManager _fieldManager;

		protected ImageDataSourceSnapshotExecutorBase(IImageFileRepository imageFileRepository,
			IFieldManager fieldManager)
		{
			_imageFileRepository = imageFileRepository;
			_fieldManager = fieldManager;
		}

		protected Task<ImagesStatistics> CreateCalculateImagesTotalSizeTaskAsync(T configuration, CancellationToken token, QueryRequest queryRequest)
		{
			QueryImagesOptions options = new QueryImagesOptions
			{
				ProductionIds = configuration.ProductionImagePrecedence,
				IncludeOriginalImageIfNotFoundInProductions = configuration.IncludeOriginalImageIfNotFoundInProductions
			};

			Task<ImagesStatistics> calculateImagesTotalSizeTask = Task.Run(() => _imageFileRepository.CalculateImagesStatisticsAsync(configuration.SourceWorkspaceArtifactId, queryRequest, options), token);
			return calculateImagesTotalSizeTask;
		}

		protected async Task<QueryRequest> CreateQueryRequestAsync(T configuration, CancellationToken token)
		{
			FieldInfoDto identifierField = await _fieldManager.GetObjectIdentifierFieldAsync(token).ConfigureAwait(false);

			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
				},
				Condition = CreateImageQueryCondition(configuration),
				Fields = new[]
				{
					new FieldRef { Name = identifierField.SourceFieldName }
				}
			};
			return queryRequest;
		}

		protected string DocumentsInSavedSearch(int savedSearchArtifactId) =>
			$"('ArtifactId' IN SAVEDSEARCH {savedSearchArtifactId})";

		protected string DocumentsWithImages(T configuration)
		{
			if (configuration.IsProductionImagePrecedenceSet)
			{
				return configuration.IncludeOriginalImageIfNotFoundInProductions
					? $"({_DOCUMENTS_WITH_PRODUCED_IMAGES} OR {_DOCUMENTS_WITH_ORIGINAL_IMAGES})"
					: _DOCUMENTS_WITH_PRODUCED_IMAGES;
			}

			return _DOCUMENTS_WITH_ORIGINAL_IMAGES;
		}

		protected abstract string CreateImageQueryCondition(T configuration);
	}
}