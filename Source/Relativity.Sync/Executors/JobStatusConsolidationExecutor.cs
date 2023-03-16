using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Executors
{
    internal class JobStatusConsolidationExecutor : IExecutor<IJobStatusConsolidationConfiguration>
    {
        private readonly IRdoGuidConfiguration _rdoGuidConfiguration;
        private readonly IBatchRepository _batchRepository;
        private readonly IJobStatisticsContainer _jobStatisticsContainer;
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IIAPIv2RunChecker _iapiv2Check;
        private readonly IAPILog _logger;

        public JobStatusConsolidationExecutor(
            IRdoGuidConfiguration rdoGuidConfiguration,
            IBatchRepository batchRepository,
            IJobStatisticsContainer jobStatisticsContainer,
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin,
            IIAPIv2RunChecker iapiv2Check,
            IAPILog logger)
        {
            _rdoGuidConfiguration = rdoGuidConfiguration;
            _batchRepository = batchRepository;
            _jobStatisticsContainer = jobStatisticsContainer;
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _iapiv2Check = iapiv2Check;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(IJobStatusConsolidationConfiguration configuration, CompositeCancellationToken token)
        {
            UpdateResult updateResult;
            try
            {
                int completedItemsCount = 0;
                int totalItemsCount = 0;
                int failedItemsCount = 0;
                int readItemsCount = 0;
                var exportRunId = configuration.ExportRunId;
                if (exportRunId.HasValue)
                {
                    List<IBatch> batches = (await _batchRepository
                        .GetAllAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId, exportRunId.Value)
                        .ConfigureAwait(false)).ToList();

                    if (_iapiv2Check.ShouldBeUsed())
                    {
                        completedItemsCount = batches.Sum(batch => batch.TransferredDocumentsCount);
                        totalItemsCount = batches.Sum(batch => batch.TotalDocumentsCount);
                        failedItemsCount = batches.Sum(batch => batch.FailedDocumentsCount);
                        readItemsCount = batches.Sum(batch => batch.ReadDocumentsCount);
                    }
                    else
                    {
                        completedItemsCount = batches.Sum(batch => batch.TransferredItemsCount);
                        totalItemsCount = await GetTotalItemsCountAsync(batches).ConfigureAwait(false);
                        failedItemsCount = batches.Sum(batch => batch.FailedItemsCount);
                        readItemsCount = batches.Sum(batch => batch.ReadDocumentsCount);
                    }
                }

                updateResult = await UpdateJobHistoryAsync(configuration, completedItemsCount, readItemsCount, failedItemsCount, totalItemsCount).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return ExecutionResult.Failure("Failed to update Job History object.", e);
            }

            if (updateResult.EventHandlerStatuses.Any(status => !status.Success))
            {
                string message = CreateEventHandlersFailureMessage(updateResult);
                return ExecutionResult.Failure(message, null);
            }

            return ExecutionResult.Success();
        }

        private async Task<int> GetTotalItemsCountAsync(List<IBatch> batches)
        {
            if (_jobStatisticsContainer.ImagesStatistics != null)
            {
                return (int)(await _jobStatisticsContainer.ImagesStatistics.ConfigureAwait(false)).TotalCount;
            }

            return batches.Sum(batch => batch.TotalDocumentsCount);
        }

        private static string CreateEventHandlersFailureMessage(UpdateResult updateResult)
        {
            IEnumerable<string> eventHandlerMessages = updateResult.EventHandlerStatuses.Select(status => status.Message);
            string joinedEventHandlerMessages = string.Join(", ", eventHandlerMessages);
            string message = $"Event handlers failed when updating Job History object: {joinedEventHandlerMessages}.";
            return message;
        }

        private async Task<UpdateResult> UpdateJobHistoryAsync(IJobStatusConsolidationConfiguration configuration, int completedItemsCount, int readItemsCount, int failedItemsCount, int totalItemsCount)
        {
            _logger.LogInformation(
                "Update JobHistory in JobStatusConsolidationExecutor - " +
                "CompletedItems: {completed}, ReadItems: {read}, FailedItems: {failed}, Total: {total}",
                completedItemsCount,
                readItemsCount,
                failedItemsCount,
                totalItemsCount);

            using (var objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var updateRequest = new UpdateRequest
                {
                    Object = new RelativityObjectRef
                    {
                        ArtifactID = configuration.JobHistoryArtifactId
                    },
                    FieldValues = new[]
                    {
                        new FieldRefValuePair
                        {
                            Field = new FieldRef
                            {
                                Guid = _rdoGuidConfiguration.JobHistory.CompletedItemsFieldGuid
                            },
                            Value = completedItemsCount
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef
                            {
                                Guid = _rdoGuidConfiguration.JobHistory.ReadItemsFieldGuid
                            },
                            Value = readItemsCount
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef
                            {
                                Guid = _rdoGuidConfiguration.JobHistory.FailedItemsFieldGuid
                            },
                            Value = failedItemsCount
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef
                            {
                                Guid = _rdoGuidConfiguration.JobHistory.TotalItemsFieldGuid
                            },
                            Value = totalItemsCount
                        }
                    }
                };

                return await objectManager
                    .UpdateAsync(configuration.SourceWorkspaceArtifactId, updateRequest)
                    .ConfigureAwait(false);
            }
        }
    }
}
