using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory
{
    public class JobHistoryService : IJobHistoryService
    {
        private readonly IKeplerServiceFactory _keplerServiceFactory;
        private readonly IAPILog _logger;

        public JobHistoryService(IKeplerServiceFactory keplerServiceFactory, IAPILog logger)
        {
            _keplerServiceFactory = keplerServiceFactory;
            _logger = logger.ForContext<JobHistoryService>();
        }

        public async Task UpdateStatusAsync(int workspaceId, int jobHistoryId, ChoiceRef status)
        {
            try
            {
                FieldRefValuePair[] fieldsToUpdate = new[]
                {
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef()
                        {
                            Guid = JobHistoryFieldGuids.JobStatusGuid
                        },
                        Value = status
                    }
                };

                await UpdateJobHistoryAsync(workspaceId, jobHistoryId, fieldsToUpdate).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Job History status. Job History ID: {jobHistoryId}", jobHistoryId);
                throw;
            }
        }

        public async Task SetTotalItemsAsync(int workspaceId, int jobHistoryId, int totalItemsCount)
        {
            try
            {
                FieldRefValuePair[] fieldsToUpdate = new[]
                {
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef()
                        {
                            Guid = JobHistoryFieldGuids.TotalItemsGuid
                        },
                        Value = totalItemsCount
                    }
                };

                await UpdateJobHistoryAsync(workspaceId, jobHistoryId, fieldsToUpdate).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set total items count: {totalItemsCount} for Job History ID: {jobHistoryId}",
                    totalItemsCount, jobHistoryId);
                throw;
            }
        }

        public async Task UpdateProgressAsync(int workspaceId, int jobHistoryId, int readItemsCount, int transferredItemsCount)
        {
            try
            {
                FieldRefValuePair[] fieldsToUpdate = new[]
                {
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef()
                        {
                            Guid = JobHistoryFieldGuids.ItemsReadGuid
                        },
                        Value = readItemsCount
                    },
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef()
                        {
                            Guid = JobHistoryFieldGuids.ItemsTransferredGuid
                        },
                        Value = transferredItemsCount
                    }
                };

                await UpdateJobHistoryAsync(workspaceId, jobHistoryId, fieldsToUpdate).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set read items count: {readItemsCount} and transferred items count: {transferredItemsCount} for Job History ID: {jobHistoryId}",
                    readItemsCount, transferredItemsCount, jobHistoryId);
                throw;
            }
        }

        private async Task UpdateJobHistoryAsync(int workspaceId, int jobHistoryId, FieldRefValuePair[] fieldsToUpdate)
        {
            using (IObjectManager objectManager =
                   await _keplerServiceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                UpdateRequest updateRequest = new UpdateRequest()
                {
                    Object = new RelativityObjectRef()
                    {
                        ArtifactID = jobHistoryId
                    },
                    FieldValues = fieldsToUpdate
                };
                UpdateResult updateResult = await objectManager.UpdateAsync(workspaceId, updateRequest).ConfigureAwait(false);

                if (!updateResult.EventHandlerStatuses.TrueForAll(x => x.Success))
                {
                    _logger.LogError(
                        "Failed to update Job History. Update request: {@request} Event handler statuses: {@eventHandlerStatuses}",
                        updateRequest, updateResult.EventHandlerStatuses);
                }
            }
        }
    }
}