using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal class NonDocumentObjectDataSourceSnapshotExecutor : IExecutor<INonDocumentDataSourceSnapshotConfiguration>
	{
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;
		private readonly ISyncLog _logger;

		public NonDocumentObjectDataSourceSnapshotExecutor(ISourceServiceFactoryForUser serviceFactory, IJobProgressUpdaterFactory jobProgressUpdaterFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_jobProgressUpdaterFactory = jobProgressUpdaterFactory;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(INonDocumentDataSourceSnapshotConfiguration configuration, CompositeCancellationToken token)
		{
			_logger.LogInformation("Initializing export of non-document objects in workspace {workspaceId} with view {viewId}",
				configuration.SourceWorkspaceArtifactId, configuration.DataSourceArtifactId);

			ExportInitializationResults exportResults;
			try
			{
				using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					QueryRequest queryRequest = BuildSnapshotQueryRequest(configuration);
					exportResults = await objectManager.InitializeExportAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 1).ConfigureAwait(false);
					_logger.LogInformation("Retrieved {objectsCount} objects from view {viewId}.", exportResults.RecordCount, configuration.DataSourceArtifactId);
				}
			}
			catch (Exception ex)
			{
				const string message = "ExportAPI failed to initialize export";
				_logger.LogError(ex, message);
				return ExecutionResult.Failure(message, ex);
			}

			await configuration.SetSnapshotDataAsync(exportResults.RunID, (int)exportResults.RecordCount);

			IJobProgressUpdater jobProgressUpdater = _jobProgressUpdaterFactory.CreateJobProgressUpdater();
			await jobProgressUpdater.SetTotalItemsCountAsync((int)exportResults.RecordCount).ConfigureAwait(false);

			return ExecutionResult.Success();
		}

		private QueryRequest BuildSnapshotQueryRequest(INonDocumentDataSourceSnapshotConfiguration configuration)
		{
			QueryRequest request = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = configuration.RdoArtifactTypeId
				},
				Condition = $"('ArtifactId' IN VIEW {configuration.DataSourceArtifactId})",
				IncludeNameInQueryResult = true
			};
			return request;
		}
	}
}
