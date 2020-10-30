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
	internal sealed class ImageDataSourceSnapshotExecutor : ImageDataSourceSnapshotExecutorBase<IImageDataSourceSnapshotConfiguration>, IExecutor<IImageDataSourceSnapshotConfiguration>
	{
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly ISyncLog _logger;

		public ImageDataSourceSnapshotExecutor(ISourceServiceFactoryForUser serviceFactory, IImageFileRepository imageFileRepository,
			IJobStatisticsContainer jobStatisticsContainer, IFieldManager fieldManager, ISyncLog logger)
		: base(imageFileRepository, fieldManager)
		{
			_serviceFactory = serviceFactory;
			_jobStatisticsContainer = jobStatisticsContainer;
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

					_jobStatisticsContainer.ImagesStatistics = CreateCalculateImagesTotalSizeTaskAsync(configuration, token, queryRequest);
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

			return ExecutionResult.Success();
		}

		protected override string CreateImageQueryCondition(IImageDataSourceSnapshotConfiguration configuration)
			=> $"{DocumentsInSavedSearch(configuration.DataSourceArtifactId)} AND {DocumentsWithImages(configuration)}";
	}
}
