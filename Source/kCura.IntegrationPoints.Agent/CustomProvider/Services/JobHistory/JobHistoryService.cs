﻿using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory
{
    public class JobHistoryService : IJobHistoryService
    {
        private readonly IKeplerServiceFactory _keplerServiceFactory;
        private readonly IDateTime _dateTime;
        private readonly IAPILog _logger;

        public JobHistoryService(IKeplerServiceFactory keplerServiceFactory, IDateTime dateTime, IAPILog logger)
        {
            _keplerServiceFactory = keplerServiceFactory;
            _dateTime = dateTime;
            _logger = logger.ForContext<JobHistoryService>();
        }

        public async Task UpdateStatusAsync(int workspaceId, int jobHistoryId, Guid statusGuid)
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
                        Value = new ChoiceRef { Guid = statusGuid }
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

        public async Task TryUpdateStartTimeAsync(int workspaceId, int jobHistoryId)
        {
            DateTime startTime = _dateTime.UtcNow;

            try
            {
                FieldRefValuePair[] fieldsToUpdate = new[]
                {
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef()
                        {
                            Guid = JobHistoryFieldGuids.StartTimeUTCGuid
                        },
                        Value = startTime
                    }
                };

                await UpdateJobHistoryAsync(workspaceId, jobHistoryId, fieldsToUpdate).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set Start Time UTC: {startTime} for Job History ID: {jobHistoryId}", startTime, jobHistoryId);
            }
        }

        public async Task TryUpdateEndTimeAsync(int workspaceId, int jobHistoryId)
        {
            DateTime endTime = _dateTime.UtcNow;

            try
            {
                FieldRefValuePair[] fieldsToUpdate = new[]
                {
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef()
                        {
                            Guid = JobHistoryFieldGuids.EndTimeUTCGuid
                        },
                        Value = endTime
                    }
                };

                await UpdateJobHistoryAsync(workspaceId, jobHistoryId, fieldsToUpdate).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set End Time UTC: {endTime} for Job History ID: {jobHistoryId}", endTime, jobHistoryId);
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

        public async Task UpdateReadItemsCountAsync(int workspaceId, int jobHistoryId, int readItemsCount)
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
                    }
                };

                await UpdateJobHistoryAsync(workspaceId, jobHistoryId, fieldsToUpdate).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set read items count: {readItemsCount} for Job History ID: {jobHistoryId}",
                    readItemsCount, jobHistoryId);
                throw;
            }
        }

        public async Task UpdateProgressAsync(int workspaceId, int jobHistoryId, int importedItemsCount, int failedItemsCount)
        {
            try
            {
                FieldRefValuePair[] fieldsToUpdate = new[]
                {
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef()
                        {
                            Guid = JobHistoryFieldGuids.ItemsTransferredGuid
                        },
                        Value = importedItemsCount
                    },
                    new FieldRefValuePair()
                    {
                        Field = new FieldRef()
                        {
                            Guid = JobHistoryFieldGuids.ItemsWithErrorsGuid
                        },
                        Value = failedItemsCount
                    }
                };

                await UpdateJobHistoryAsync(workspaceId, jobHistoryId, fieldsToUpdate).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set imported items count: {importedItemsCount} and failed items count: {failedItemsCount} for Job History ID: {jobHistoryId}",
                    importedItemsCount, failedItemsCount, jobHistoryId);
                throw;
            }
        }

        public async Task<Data.JobHistory> ReadJobHistoryByGuidAsync(int workspaceId, Guid batchInstanceId)
        {
            using (IObjectManager objectManager =
                   await _keplerServiceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.JobHistoryGuid },
                    Condition = $"'{JobHistoryFields.BatchInstance}' == '{batchInstanceId}'",
                    Fields = RDOConverter.GetFieldList<Data.JobHistory>()
                };

                QueryResult result = await objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue).ConfigureAwait(false);

                RelativityObject obj = result.Objects.SingleOrDefault();

                return obj?.ToRDO<Data.JobHistory>();
            }
        }

        public async Task<int> CreateJobHistoryAsync(int workspaceId, Data.JobHistory jobHistory)
        {
            using (IObjectManager objectManager = await _keplerServiceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                CreateRequest request = new CreateRequest
                {
                    ObjectType = jobHistory.ToObjectType(),
                    FieldValues = jobHistory.ToFieldValues()
                };

                CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);

                return result.Object.ArtifactID;
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
