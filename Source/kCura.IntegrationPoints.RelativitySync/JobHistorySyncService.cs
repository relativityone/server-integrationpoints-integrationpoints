using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Extensions;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.RelativitySync
{
    internal class JobHistorySyncService : IJobHistorySyncService
    {
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly IToggleProvider _toggles;
        private readonly IAPILog _logger;

        public JobHistorySyncService(IRelativityObjectManager relativityObjectManager, IToggleProvider toggles, IAPILog logger)
        {
            _relativityObjectManager = relativityObjectManager;
            _toggles = toggles;
            _logger = logger.ForContext<JobHistorySyncService>();
        }

        public async Task<RelativityObject> GetLastJobHistoryWithErrorsAsync(int workspaceID,
            int integrationPointArtifactID)
        {
            string integrationPointCondition = $"('{JobHistoryFields.IntegrationPoint}' INTERSECTS MULTIOBJECT [{integrationPointArtifactID}])";
            string notRunningCondition = $"('{JobHistoryFields.EndTimeUTC}' ISSET)";
            string jobStatusCondition = $"('{JobHistoryFields.JobStatus}' IN CHOICE [{JobStatusChoices.JobHistoryCompletedWithErrorsGuid}, {JobStatusChoices.JobHistoryErrorJobFailedGuid}])";
            string condition = $"{integrationPointCondition} AND {notRunningCondition} AND {jobStatusCondition}";

            var queryRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef()
                {
                    Guid = ObjectTypeGuids.JobHistoryGuid
                },
                Condition = condition,
                Fields = new[]
                {
                    new FieldRef
                    {
                        Guid = JobHistoryFieldGuids.IntegrationPointGuid
                    }
                },
                Sorts = new List<Sort>
                {
                    new Sort
                    {
                        Direction = SortEnum.Descending,
                        FieldIdentifier = new FieldRef
                        {
                            Guid = JobHistoryFieldGuids.EndTimeUTCGuid
                        }
                    }
                }
            };

            List<RelativityObject> results = await _relativityObjectManager.QueryAsync(queryRequest, executionIdentity: ExecutionIdentity.System).ConfigureAwait(false);
            return results.FirstOrDefault();
        }

        public async Task UpdateJobStatusAsync(string syncStatus, IExtendedJob job)
        {
            if (SyncUpdatesJobHistory())
            {
                return;
            }

            ChoiceRef status;

            const string validating = "validating";
            const string checkingPermissions = "checking permissions";

            if (syncStatus.Equals(validating, StringComparison.InvariantCultureIgnoreCase) || syncStatus.Equals(checkingPermissions, StringComparison.InvariantCultureIgnoreCase))
            {
                status = new ChoiceRef
                {
                    Guid = JobStatusChoices.JobHistoryValidating.Guids[0]
                };
            }
            else
            {
                status = new ChoiceRef
                {
                    Guid = JobStatusChoices.JobHistoryProcessing.Guids[0]
                };
            }

            IList<FieldRefValuePair> fieldRefValuePair = new[]
            {
                new FieldRefValuePair
                {
                    Field = JobStatusRef(),
                    Value = status
                }
            };

            await _relativityObjectManager.UpdateAsync(job.JobHistoryId, fieldRefValuePair, ExecutionIdentity.System).ConfigureAwait(false);

        }

        public async Task MarkJobAsValidationFailedAsync(IExtendedJob job, Exception ex)
        {
            if (SyncUpdatesJobHistory())
            {
                return;
            }

            await UpdateFinishedJobAsync(job, JobValidationFailedRef(), true).ConfigureAwait(false);
            await AddJobHistoryErrorAsync(job, ex).ConfigureAwait(false);
        }

        public async Task MarkJobAsStoppedAsync(IExtendedJob job)
        {
            if (SyncUpdatesJobHistory())
            {
                return;
            }

            bool hasErrors = await HasErrorsAsync(job).ConfigureAwait(false);
            await UpdateFinishedJobAsync(job, JobStoppedStateRef(), hasErrors).ConfigureAwait(false);
        }

        public async Task MarkJobAsSuspendingAsync(IExtendedJob job)
        {
            if (SyncUpdatesJobHistory())
            {
                return;
            }

            bool hasErrors = await HasErrorsAsync(job).ConfigureAwait(false);
            await UpdateFinishedJobAsync(job, JobSuspendingStateRef(), hasErrors).ConfigureAwait(false);
        }

        public async Task MarkJobAsSuspendedAsync(IExtendedJob job)
        {
            if (SyncUpdatesJobHistory())
            {
                return;
            }

            bool hasErrors = await HasErrorsAsync(job).ConfigureAwait(false);
            await UpdateFinishedJobAsync(job, JobSuspendedStateRef(), hasErrors).ConfigureAwait(false);
        }

        public async Task MarkJobAsFailedAsync(IExtendedJob job, Exception e)
        {
            if (SyncUpdatesJobHistory())
            {
                return;
            }

            await MarkJobAsFailedAsync(job).ConfigureAwait(false);
            await AddJobHistoryErrorAsync(job, e).ConfigureAwait(false);
        }

        public async Task MarkJobAsStartedAsync(IExtendedJob job)
        {
            // We must set Job ID and Start Time here regardless of EnableJobHistoryStatusUpdateToggle

            IList<FieldRefValuePair> fieldValues = new[]
            {
                new FieldRefValuePair
                {
                    Field = StartTimeRef(),
                    Value = DateTime.UtcNow
                },
                new FieldRefValuePair
                {
                    Field = JobIdRef(),
                    Value = job.JobId.ToString(CultureInfo.InvariantCulture)
                }
            };

            await _relativityObjectManager.UpdateAsync(job.JobHistoryId, fieldValues, ExecutionIdentity.System).ConfigureAwait(false);

        }

        public async Task MarkJobAsCompletedAsync(IExtendedJob job)
        {
            if (SyncUpdatesJobHistory())
            {
                return;
            }

            ChoiceRef status;
            bool hasErrors = await HasErrorsAsync(job).ConfigureAwait(false);
            if (hasErrors)
            {
                status = JobCompletedWithErrorsStateRef();
            }
            else
            {
                status = JobCompletedStateRef();
            }

            await UpdateFinishedJobAsync(job, status, hasErrors).ConfigureAwait(false);
        }

        private async Task<bool> HasErrorsAsync(IExtendedJob job)
        {
            QueryRequest request = new QueryRequest
            {
                ObjectType = JobHistoryErrorTypeRef(),
                Condition =
                    $"('{Data.JobHistoryErrorFields.JobHistory}' IN OBJECT [{job.JobHistoryId}]) AND ('{Data.JobHistoryErrorFields.ErrorType}' == CHOICE {ErrorTypeChoices.JobHistoryErrorItem.Guids[0]})"
            };
            Data.UtilityDTO.ResultSet<RelativityObject> itemLevelErrors = await _relativityObjectManager.QueryAsync(request, 0, 1).ConfigureAwait(false);
            _logger.LogInformation("JobHistorySyncService.HasErrors(): Found {itemLevelErrors} from JobHistoryErrorObjects", itemLevelErrors.ResultCount);

            QueryRequest requestForJobHistory = new QueryRequest()
            {
                ObjectType = JobHistoryRef(),
                Condition = $"'Artifact ID' == '{job.JobHistoryId}'"
            };

            List<JobHistory> jobHistoryFromQuery =
                await _relativityObjectManager.QueryAsync<JobHistory>(requestForJobHistory).ConfigureAwait(false);

            int? jobHistoryItemsWithErrors = jobHistoryFromQuery.Single().ItemsWithErrors;

            _logger.LogInformation("JobHistorySyncService.HasErrors(): Found {jobHistoryItemsWithErrors} from JobHistory.ItemsWithErrors", jobHistoryItemsWithErrors);

            return itemLevelErrors.ResultCount > 0 || jobHistoryItemsWithErrors > 0;
        }

        private async Task MarkJobAsFailedAsync(IExtendedJob job)
        {
            await UpdateFinishedJobAsync(job, JobFailedStateRef(), true).ConfigureAwait(false);
        }

        private async Task UpdateFinishedJobAsync(IExtendedJob job, ChoiceRef status, bool hasErrors)
        {
            DateTime currentTimeUtc = DateTime.UtcNow;
            IList<FieldRefValuePair> fieldValues = new[]
            {
                new FieldRefValuePair
                {
                    Field = JobStatusRef(),
                    Value = status
                },
                new FieldRefValuePair
                {
                    Field = EndTimeRef(),
                    Value = currentTimeUtc
                }
            };

            await _relativityObjectManager.UpdateAsync(job.JobHistoryId, fieldValues, ExecutionIdentity.System).ConfigureAwait(false);
            await UpdateIntegrationPointLastRuntimeUtcAsync(job, currentTimeUtc).ConfigureAwait(false);
            await UpdateIntegrationPointHasErrorsAsync(job, hasErrors).ConfigureAwait(false);
        }

        private async Task UpdateIntegrationPointLastRuntimeUtcAsync(IExtendedJob job, DateTime currentTimeUtc)
        {
            IList<FieldRefValuePair> fieldValues = new[]
            {
                new FieldRefValuePair
                {
                    Field = LastRuntimeUtcRef(),
                    Value = currentTimeUtc
                },
            };

            await _relativityObjectManager.UpdateAsync(job.IntegrationPointId, fieldValues, ExecutionIdentity.System).ConfigureAwait(false);
        }

        private async Task UpdateIntegrationPointHasErrorsAsync(IExtendedJob job, bool hasErrors)
        {
            IList<FieldRefValuePair> fieldValues = new[]
            {
                new FieldRefValuePair
                {
                    Field = HasErrorsRef(),
                    Value = hasErrors
                },
            };

            await _relativityObjectManager.UpdateAsync(job.IntegrationPointId, fieldValues, ExecutionIdentity.System).ConfigureAwait(false);
        }

        private bool SyncUpdatesJobHistory()
        {
            return _toggles.IsEnabledByName("Relativity.Sync.Toggles.EnableJobHistoryStatusUpdateToggle");
        }

        private static FieldRef JobIdRef()
        {
            return new FieldRef
            {
                Guid = JobHistoryFieldGuids.JobIDGuid
            };
        }

        private static FieldRef EndTimeRef()
        {
            return new FieldRef
            {
                Guid = JobHistoryFieldGuids.EndTimeUTCGuid
            };
        }

        private static RelativityObjectRef JobHistoryRef(IExtendedJob job)
        {
            return new RelativityObjectRef
            {
                ArtifactID = job.JobHistoryId
            };
        }

        private static ObjectTypeRef JobHistoryRef()
        {
            return new ObjectTypeRef
            {
                Guid = ObjectTypeGuids.JobHistoryGuid
            };
        }

        private static FieldRef LastRuntimeUtcRef()
        {
            return new FieldRef
            {
                Guid = IntegrationPointFieldGuids.LastRuntimeUTCGuid
            };
        }

        private static FieldRef HasErrorsRef()
        {
            return new FieldRef()
            {
                Guid = IntegrationPointFieldGuids.HasErrorsGuid
            };
        }

        private static FieldRef JobStatusRef()
        {
            return new FieldRef
            {
                Guid = JobHistoryFieldGuids.JobStatusGuid
            };
        }

        private static FieldRef StartTimeRef()
        {
            return new FieldRef
            {
                Guid = JobHistoryFieldGuids.StartTimeUTCGuid
            };
        }

        private static ChoiceRef JobValidationFailedRef()
        {
            return new ChoiceRef()
            {
                Guid = JobStatusChoices.JobHistoryValidationFailed.Guids[0]
            };
        }

        private static ChoiceRef JobStoppedStateRef()
        {
            return new ChoiceRef
            {
                Guid = JobStatusChoices.JobHistoryStopped.Guids[0]
            };
        }

        private static ChoiceRef JobSuspendingStateRef()
        {
            return new ChoiceRef
            {
                Guid = JobStatusChoices.JobHistorySuspending.Guids[0]
            };
        }

        private static ChoiceRef JobSuspendedStateRef()
        {
            return new ChoiceRef
            {
                Guid = JobStatusChoices.JobHistorySuspended.Guids[0]
            };
        }

        private static ChoiceRef JobCompletedStateRef()
        {
            return new ChoiceRef
            {
                Guid = JobStatusChoices.JobHistoryCompleted.Guids[0]
            };
        }

        private static ChoiceRef JobCompletedWithErrorsStateRef()
        {
            return new ChoiceRef
            {
                Guid = JobStatusChoices.JobHistoryCompletedWithErrors.Guids[0]
            };
        }

        private static ChoiceRef JobFailedStateRef()
        {
            return new ChoiceRef
            {
                Guid = JobStatusChoices.JobHistoryErrorJobFailed.Guids[0]
            };
        }

        private async Task AddJobHistoryErrorAsync(IExtendedJob job, Exception e)
        {

            ObjectTypeRef objectType = JobHistoryErrorTypeRef();
            RelativityObjectRef parentObject = JobHistoryRef(job);
            List<FieldRefValuePair> fieldValues = JobHistoryErrorFields(e);

            await _relativityObjectManager.CreateAsync(objectType, parentObject, fieldValues, ExecutionIdentity.System).ConfigureAwait(false);
        }

        private static ObjectTypeRef JobHistoryErrorTypeRef()
        {
            return new ObjectTypeRef
            {
                Guid = ObjectTypeGuids.JobHistoryErrorGuid
            };
        }

        private static List<FieldRefValuePair> JobHistoryErrorFields(Exception e)
        {
            return new List<FieldRefValuePair>
            {
                ErrorField(e),
                ErrorStatus(),
                ErrorType(),
                StackTrace(e),
                Name()
            };
        }

        private static FieldRefValuePair ErrorField(Exception ex)
        {
            return new FieldRefValuePair
            {
                Field = new FieldRef
                {
                    Guid = JobHistoryErrorFieldGuids.ErrorGuid
                },
                Value = ex.FlattenErrorMessages()
            };
        }

        private static FieldRefValuePair ErrorStatus()
        {
            return new FieldRefValuePair
            {
                Field = new FieldRef
                {
                    Guid = JobHistoryErrorFieldGuids.ErrorStatusGuid
                },
                Value = new ChoiceRef
                {
                    Guid = ErrorStatusChoices.JobHistoryErrorNew.Guids[0]
                }
            };
        }

        private static FieldRefValuePair ErrorType()
        {
            return new FieldRefValuePair
            {
                Field = new FieldRef
                {
                    Guid = JobHistoryErrorFieldGuids.ErrorTypeGuid
                },
                Value = new ChoiceRef
                {
                    Guid = ErrorTypeChoices.JobHistoryErrorJob.Guids[0]
                }
            };
        }

        private static FieldRefValuePair StackTrace(Exception e)
        {
            return new FieldRefValuePair
            {
                Field = new FieldRef
                {
                    Guid = JobHistoryErrorFieldGuids.StackTraceGuid
                },
                Value = e.ToString()
            };
        }

        private static FieldRefValuePair Name()
        {
            return new FieldRefValuePair
            {
                Field = new FieldRef
                {
                    Guid = JobHistoryErrorFieldGuids.NameGuid
                },
                Value = Guid.NewGuid().ToString()
            };
        }
    }
}