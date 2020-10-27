using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal sealed class ImageDataSourceSnapshotExecutor : IExecutor<IImageDataSourceSnapshotConfiguration>
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int) ArtifactType.Document;
		private const int _HAS_IMAGES_YES_CHOICE = 1034243;
		private const string _HAS_IMAGES_FIELD_NAME = "Has Images";
		private const string _PRODUCTION_IMAGE_COUNT_FIELD_NAME = "Production::Image Count";

		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;
		private readonly IImageFileRepository _imageFileRepository;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly ISyncLog _logger;

		public ImageDataSourceSnapshotExecutor(ISourceServiceFactoryForUser serviceFactory,
			IJobProgressUpdaterFactory jobProgressUpdaterFactory,
			IImageFileRepository imageFileRepository, IJobStatisticsContainer jobStatisticsContainer, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_jobProgressUpdaterFactory = jobProgressUpdaterFactory;
			_imageFileRepository = imageFileRepository;
			_jobStatisticsContainer = jobStatisticsContainer;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IImageDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation(
				"Initializing image export in workspace {workspaceId} with saved search {savedSearchId}.",
				configuration.SourceWorkspaceArtifactId, configuration.DataSourceArtifactId);

			QueryRequest queryRequest = CreateQueryRequest(configuration);

			ExportInitializationResults results;
			try
			{
				using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					results = await objectManager
						.InitializeExportAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 1)
						.ConfigureAwait(false);
					_logger.LogInformation("Retrieved {documentCount} documents from saved search which have images",
						results.RecordCount);

					QueryImagesOptions options = new QueryImagesOptions
					{
						ProductionIds = configuration.ProductionIds,
						IncludeOriginalImageIfNotFoundInProductions =
							configuration.IncludeOriginalImageIfNotFoundInProductions
					};

					Task<long> calculateImagesTotalSizeTask = Task.Run(() =>
						_imageFileRepository.CalculateImagesTotalSizeAsync(configuration.SourceWorkspaceArtifactId, queryRequest, options), token);
					_jobStatisticsContainer.ImagesBytesRequested = calculateImagesTotalSizeTask;
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "ExportAPI failed to initialize export.");
				return ExecutionResult.Failure("ExportAPI failed to initialize export.", e);
			}

			//ExportInitializationResult provide list of fields with order they will be returned when retrieving metadata
			//however, order is the same as order of fields in QueryRequest when they are provided explicitly
			await configuration.SetSnapshotDataAsync(results.RunID, (int) results.RecordCount).ConfigureAwait(false);

			IJobProgressUpdater jobProgressUpdater = _jobProgressUpdaterFactory.CreateJobProgressUpdater();
			await jobProgressUpdater.SetTotalItemsCountAsync((int) results.RecordCount).ConfigureAwait(false);

			return ExecutionResult.Success();
		}

		private QueryRequest CreateQueryRequest(IImageDataSourceSnapshotConfiguration configuration)
		{
			string imageCondition = configuration.ProductionIds.Any()
				? $"('{_PRODUCTION_IMAGE_COUNT_FIELD_NAME}' > 0)"
				: $"('{_HAS_IMAGES_FIELD_NAME}' == CHOICE {_HAS_IMAGES_YES_CHOICE})";

			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
				},
				Condition = $"('ArtifactId' IN SAVEDSEARCH {configuration.DataSourceArtifactId}) AND {imageCondition}"
			};
			return queryRequest;
		}
	}
}