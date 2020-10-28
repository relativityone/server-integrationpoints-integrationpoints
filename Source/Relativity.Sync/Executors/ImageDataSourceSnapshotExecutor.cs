using System;
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
	internal sealed class ImageDataSourceSnapshotExecutor : ImageDataSourceSnapshotExecutorBase, IExecutor<IImageDataSourceSnapshotConfiguration>
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int) ArtifactType.Document;

		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly IFieldManager _fieldManager;
		private readonly ISyncLog _logger;

		public ImageDataSourceSnapshotExecutor(ISourceServiceFactoryForUser serviceFactory, IJobProgressUpdaterFactory jobProgressUpdaterFactory,
			IImageFileRepository imageFileRepository, IJobStatisticsContainer jobStatisticsContainer, IFieldManager fieldManager, ISyncLog logger)
		: base(imageFileRepository)
		{
			_serviceFactory = serviceFactory;
			_jobProgressUpdaterFactory = jobProgressUpdaterFactory;
			_jobStatisticsContainer = jobStatisticsContainer;
			_fieldManager = fieldManager;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IImageDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation(
				"Initializing image export in workspace {workspaceId} with saved search {savedSearchId}.",
				configuration.SourceWorkspaceArtifactId, configuration.DataSourceArtifactId);
			
			QueryRequest queryRequest = await CreateQueryRequestAsync(configuration, token).ConfigureAwait(false);

			ExportInitializationResults results;
			try
			{
				using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					results = await objectManager
						.InitializeExportAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 1)
						.ConfigureAwait(false);
					_logger.LogInformation("Retrieved {documentCount} documents from saved search which have images", results.RecordCount);

					_jobStatisticsContainer.ImagesBytesRequested = CreateCalculateImagesTotalSizeTaskAsync(configuration, token, queryRequest);
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

		private async Task<QueryRequest> CreateQueryRequestAsync(IImageDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			FieldInfoDto identifierField = await _fieldManager.GetObjectIdentifierFieldAsync(token).ConfigureAwait(false);
			string imageCondition = CreateConditionToRetrieveImages(configuration.ProductionImagePrecedence);

			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
				},
				Condition = $"('ArtifactId' IN SAVEDSEARCH {configuration.DataSourceArtifactId}) AND {imageCondition}",
				Fields = new[]
				{
					new FieldRef
					{
						Name = identifierField.SourceFieldName
					}
				}
			};
			return queryRequest;
		}
	}
}