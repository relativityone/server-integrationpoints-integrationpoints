using System;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal class DataSourceSnapshotExecutor : IExecutor<IDataSourceSnapshotConfiguration>
	{
		private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
		private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;
		private readonly ISnapshotQueryRequestProvider _snapshotQueryRequestProvider;

		protected readonly ISyncLog Logger;

		public DataSourceSnapshotExecutor(ISourceServiceFactoryForUser serviceFactoryForUser, 
			IJobProgressUpdaterFactory jobProgressUpdaterFactory, ISyncLog logger, 
			ISnapshotQueryRequestProvider snapshotQueryRequestProvider)
		{
			_serviceFactoryForUser = serviceFactoryForUser;
			_jobProgressUpdaterFactory = jobProgressUpdaterFactory;
			Logger = logger;
			_snapshotQueryRequestProvider = snapshotQueryRequestProvider;
		}

		public virtual async Task<ExecutionResult> ExecuteAsync(IDataSourceSnapshotConfiguration configuration, CompositeCancellationToken token)
		{
			Logger.LogInformation("Initializing export in workspace {workspaceId} with saved search {savedSearchId} and fields {fields}.",
				configuration.SourceWorkspaceArtifactId, configuration.DataSourceArtifactId, configuration.GetFieldMappings());

			ExportInitializationResults results;
			try
			{
				QueryRequest queryRequest = await _snapshotQueryRequestProvider.GetRequestForCurrentPipelineAsync(token.StopCancellationToken).ConfigureAwait(false);
				using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					results = await objectManager.InitializeExportAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 1).ConfigureAwait(false);
					Logger.LogInformation("Retrieved {documentsCount} documents from saved search.", results.RecordCount);
				}
			}
			catch (Exception e)
			{
				Logger.LogError(e, "ExportAPI failed to initialize export.");
				return ExecutionResult.Failure("ExportAPI failed to initialize export.", e);
			}

			//ExportInitializationResult provide list of fields with order they will be returned when retrieving metadata
			//however, order is the same as order of fields in QueryRequest when they are provided explicitly
			await configuration.SetSnapshotDataAsync(results.RunID, (int)results.RecordCount).ConfigureAwait(false);

			IJobProgressUpdater jobProgressUpdater = _jobProgressUpdaterFactory.CreateJobProgressUpdater();
			await jobProgressUpdater.SetTotalItemsCountAsync((int)results.RecordCount).ConfigureAwait(false);

			return ExecutionResult.Success();
		}
	}
}
