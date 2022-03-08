using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using System;
using System.Threading.Tasks;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class NonDocumentObjectDataSourceSnapshotExecutor : IExecutor<INonDocumentDataSourceSnapshotConfiguration>
    {
        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
        private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;
        private readonly ISnapshotQueryRequestProvider _snapshotQueryRequestProvider;
        private readonly ISyncLog _logger;

        public NonDocumentObjectDataSourceSnapshotExecutor(ISourceServiceFactoryForUser serviceFactoryForUser,
            IJobProgressUpdaterFactory jobProgressUpdaterFactory,
            ISnapshotQueryRequestProvider snapshotQueryRequestProvider, ISyncLog logger)
        {
            _serviceFactoryForUser = serviceFactoryForUser;
            _jobProgressUpdaterFactory = jobProgressUpdaterFactory;
            _snapshotQueryRequestProvider = snapshotQueryRequestProvider;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(INonDocumentDataSourceSnapshotConfiguration configuration,
            CompositeCancellationToken token)
        {
            Task<ExecutionResult> allObjectsExportTask = InitializeAllObjectsExportAsync(configuration, token);
            Task<ExecutionResult> objectsToLinkExportTask = InitializeObjectsToLinkExportAsync(configuration, token);

            ExecutionResult allObjectsExportResult = await allObjectsExportTask.ConfigureAwait(false);
            ExecutionResult objectToLinkExportResult = await objectsToLinkExportTask.ConfigureAwait(false);

            return AggregateResults(allObjectsExportResult, objectToLinkExportResult);
        }

        private ExecutionResult AggregateResults(ExecutionResult allObjectsExportResult,
            ExecutionResult objectToLinkExportResult)
        {
            return (allObjectsExportResult.Status, objectToLinkExportResult.Status) switch
            {
                (ExecutionStatus.Completed, ExecutionStatus.Completed) => ExecutionResult.Success(),
                (ExecutionStatus.Failed, ExecutionStatus.Completed) => allObjectsExportResult,
                (ExecutionStatus.Completed, ExecutionStatus.Failed) => objectToLinkExportResult,
                _ => ExecutionResult.Failure("Failed to initialize objects exports",
                    new AggregateException(allObjectsExportResult.Exception, objectToLinkExportResult.Exception))
            };
        }

        private async Task<ExecutionResult> InitializeObjectsToLinkExportAsync(
            INonDocumentDataSourceSnapshotConfiguration configuration, CompositeCancellationToken token)
        {
            try
            {
                _logger.LogInformation(
                    "Initializing export of non-document objects for linking in workspace {workspaceId} with view {viewId}",
                    configuration.SourceWorkspaceArtifactId, configuration.DataSourceArtifactId);

                QueryRequest queryRequest = await _snapshotQueryRequestProvider
                    .GetRequestForLinkingNonDocumentObjectsAsync(token.AnyReasonCancellationToken)
                    .ConfigureAwait(false);

                // if null, no objects need linking
                if (queryRequest != null)
                {
                    ExportInitializationResults exportResults;
                    using (IObjectManager objectManager =
                           await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                    {
                        exportResults = await objectManager
                            .InitializeExportAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 1)
                            .ConfigureAwait(false);

                        _logger.LogInformation("Retrieved {objectsCount} objects from view {viewId}.",
                            exportResults.RecordCount, configuration.DataSourceArtifactId);


                        // if there are no matching records, no need to save the export
                        if (exportResults.RecordCount > 0)
                        {
                            await configuration
                                .SetObjectLinkingSnapshotDataAsync(exportResults.RunID, (int)exportResults.RecordCount)
                                .ConfigureAwait(false);
                        }
                        else
                        {
                            // delete export table
                            await objectManager
                                .RetrieveResultsBlockFromExportAsync(
                                    configuration.SourceWorkspaceArtifactId,
                                    exportResults.RunID, 0, 0)
                                .ConfigureAwait(false);
                        }
                    }
                }

                return ExecutionResult.Success();
            }
            catch (Exception ex)
            {
                const string message = "ExportAPI failed to initialize export linking non-document objects";
                _logger.LogError(ex, message);
                return ExecutionResult.Failure(message, ex);
            }
        }

        private async Task<ExecutionResult> InitializeAllObjectsExportAsync(
            INonDocumentDataSourceSnapshotConfiguration configuration,
            CompositeCancellationToken token)
        {
            try
            {
                _logger.LogInformation(
                    "Initializing export of non-document objects in workspace {workspaceId} with view {viewId}",
                    configuration.SourceWorkspaceArtifactId, configuration.DataSourceArtifactId);

                ExportInitializationResults exportResults;
                using (IObjectManager objectManager =
                       await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                {
                    QueryRequest queryRequest = await _snapshotQueryRequestProvider
                        .GetRequestForCurrentPipelineAsync(token.AnyReasonCancellationToken).ConfigureAwait(false);

                    exportResults = await objectManager
                        .InitializeExportAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 1)
                        .ConfigureAwait(false);

                    _logger.LogInformation("Retrieved {objectsCount} objects from view {viewId}.",
                        exportResults.RecordCount, configuration.DataSourceArtifactId);
                }


                await configuration.SetSnapshotDataAsync(exportResults.RunID, (int)exportResults.RecordCount);

                IJobProgressUpdater jobProgressUpdater = _jobProgressUpdaterFactory.CreateJobProgressUpdater();
                await jobProgressUpdater.SetTotalItemsCountAsync((int)exportResults.RecordCount).ConfigureAwait(false);

                return ExecutionResult.Success();
            }
            catch (Exception ex)
            {
                const string message = "ExportAPI failed to initialize export for all non-document objects";
                _logger.LogError(ex, message);
                return ExecutionResult.Failure(message, ex);
            }
        }
    }
}