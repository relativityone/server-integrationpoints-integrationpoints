using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Extensions;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.RelativitySync
{
    internal class JobHistorySyncService : IJobHistorySyncService
    {
        private readonly IRelativityObjectManager _relativityObjectManager;

        public JobHistorySyncService(IRelativityObjectManager relativityObjectManager)
        {
            _relativityObjectManager = relativityObjectManager;
        }

        public async Task<RelativityObject> GetLastJobHistoryWithErrorsAsync(int workspaceId, int integrationPointArtifactId)
        {
            string integrationPointCondition = $"('{JobHistoryFields.IntegrationPoint}' INTERSECTS MULTIOBJECT [{integrationPointArtifactId}])";
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

        public async Task<DateTime?> GetLastCompletedJobHistoryForRunDateAsync(int workspaceId, int integrationPointArtifactId)
        {
            string integrationPointCondition = $"('{JobHistoryFields.IntegrationPoint}' INTERSECTS MULTIOBJECT [{integrationPointArtifactId}])";
            string notRunningCondition = $"('{JobHistoryFields.EndTimeUTC}' ISSET)";
            string jobStatusCondition = $"('{JobHistoryFields.JobStatus}' IN CHOICE [{JobStatusChoices.JobHistoryCompletedGuid}])";
            string jobTypeCondition = $"('{JobHistoryFields.JobType}' IN CHOICE [{JobTypeChoices.JobHistoryRunGuid}])";

            string condition = $"{integrationPointCondition} AND {notRunningCondition} AND {jobStatusCondition} AND {jobTypeCondition}";

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
                        Guid = JobHistoryFieldGuids.StartTimeUTCGuid
                    }
                },
                Sorts = new List<Sort>
                {
                    new Sort
                    {
                        Direction = SortEnum.Descending,
                        FieldIdentifier = new FieldRef
                        {
                            Guid = JobHistoryFieldGuids.StartTimeUTCGuid
                        }
                    }
                }
            };

            List<RelativityObject> results = await _relativityObjectManager.QueryAsync(queryRequest, executionIdentity: ExecutionIdentity.System).ConfigureAwait(false);
            RelativityObject jobHistory = results.FirstOrDefault();

            if (jobHistory != null)
            {
                DateTime startTime = (DateTime)jobHistory[JobHistoryFieldGuids.StartTimeUTCGuid].Value;
                return startTime;
            }
            else
            {
                return null;
            }
        }

        public async Task UpdateFinishedJobAsync(IExtendedJob job, ChoiceRef status, bool hasErrors)
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

        public async Task AddJobHistoryErrorAsync(IExtendedJob job, Exception e)
        {
            ObjectTypeRef objectType = JobHistoryErrorTypeRef();
            RelativityObjectRef parentObject = JobHistoryRef(job);
            List<FieldRefValuePair> fieldValues = JobHistoryErrorFields(e);

            await _relativityObjectManager.CreateAsync(
                    objectType, parentObject, fieldValues, ExecutionIdentity.System)
                .ConfigureAwait(false);
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
