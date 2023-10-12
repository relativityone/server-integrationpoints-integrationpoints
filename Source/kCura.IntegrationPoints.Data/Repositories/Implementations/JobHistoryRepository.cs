using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;

using Choice = Relativity.Services.Choice.ChoiceRef;
using Sort = Relativity.Services.Objects.DataContracts.Sort;
using SortEnum = Relativity.Services.Objects.DataContracts.SortEnum;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class JobHistoryRepository : IJobHistoryRepository
    {
        private readonly IRelativityObjectManager _relativityObjectManager;

        internal JobHistoryRepository(IRelativityObjectManager relativityObjectManager)
        {
            _relativityObjectManager = relativityObjectManager;
        }

        public int GetLastJobHistoryArtifactId(int integrationPointArtifactId)
        {
            string integrationPointCondition = CreateIntegrationPointCondition(integrationPointArtifactId);
            string notRunningCondition = $"('{JobHistoryFields.EndTimeUTC}' ISSET)";
            string condition = $"{integrationPointCondition} AND {notRunningCondition}";

            var queryRequest = new QueryRequest
            {
                Condition = condition,
                Fields = new[] { new FieldRef { Guid = Guid.Parse(JobHistoryFieldGuids.IntegrationPoint) } },
                Sorts = new List<Sort>
                {
                    new Sort
                    {
                        Direction = SortEnum.Descending,
                        FieldIdentifier = new FieldRef { Guid = Guid.Parse(JobHistoryFieldGuids.EndTimeUTC)}
                    }
                }
            };

            IEnumerable<JobHistory> result = _relativityObjectManager.Query<JobHistory>(queryRequest, 0, 1).Items;
            return result.Select(x => x.ArtifactId).FirstOrDefault();
        }

        public Choice GetLastJobHistoryStatus(int integrationPointArtifactId)
        {
            string integrationPointCondition = CreateIntegrationPointCondition(integrationPointArtifactId);

            var queryRequest = new QueryRequest
            {
                Condition = integrationPointCondition,
                Fields = new[]
                {
                    new FieldRef { Guid = Guid.Parse(JobHistoryFieldGuids.JobStatus) },
                },
                Sorts = new List<Sort>
                {
                    new Sort
                    {
                        Direction = SortEnum.Descending,
                        FieldIdentifier = new FieldRef { Name = "Artifact ID" }
                    }
                }
            };

            IEnumerable<JobHistory> result = _relativityObjectManager.Query<JobHistory>(queryRequest, 0, 1).Items;
            return result.Select(x => x.JobStatus).FirstOrDefault();
        }

        public Guid GetLastJobHistoryGuid(int integrationPointId)
        {
            string integrationPointCondition = CreateIntegrationPointCondition(integrationPointId);

            var queryRequest = new QueryRequest
            {
                Condition = integrationPointCondition,
                Fields = new[]
                {
                    new FieldRef { Guid = JobHistoryFieldGuids.IntegrationPointGuid },
                    new FieldRef { Guid = JobHistoryFieldGuids.BatchInstanceGuid },
                },
                Sorts = new List<Sort>
                {
                    new Sort
                    {
                        Direction = SortEnum.Descending,
                        FieldIdentifier = new FieldRef { Name = "Artifact ID" }
                    }
                }
            };

            IEnumerable<JobHistory> result = _relativityObjectManager.Query<JobHistory>(queryRequest, 0, 1).Items;
            return result.Select(x => new Guid(x.BatchInstance)).FirstOrDefault();
        }

        public void MarkJobAsValidationFailed(int jobHistoryID, int integrationPointID, DateTime jobEndTime)
        {
            ChoiceRef status = new ChoiceRef
            {
                Guid = JobStatusChoices.JobHistoryValidationFailed.Guids[0]
            };

            UpdateJobHistoryStatusField(jobHistoryID, status, jobEndTime);
            UpdateIntegrationPointFields(integrationPointID, jobEndTime);
        }

        public void MarkJobAsFailed(int jobHistoryID, int integrationPointID, DateTime jobEndTime)
        {
            ChoiceRef status = new ChoiceRef
            {
                Guid = JobStatusChoices.JobHistoryErrorJobFailed.Guids[0]
            };

            UpdateJobHistoryStatusField(jobHistoryID, status, jobEndTime);
            UpdateIntegrationPointFields(integrationPointID, jobEndTime);
        }

        public string GetJobHistoryName(int jobHistoryArtifactId)
        {
            IEnumerable<Guid> fieldsToRetrieve = new[] { Guid.Parse(JobHistoryFieldGuids.Name) };
            JobHistory jobHistory = _relativityObjectManager.Read<JobHistory>(jobHistoryArtifactId, fieldsToRetrieve);
            return jobHistory.Name;
        }

        public IList<JobHistory> GetStoppableJobHistoriesForIntegrationPoint(int integrationPointArtifactId)
        {
            string integrationPointCondition = CreateIntegrationPointCondition(integrationPointArtifactId);
            string stoppableCondition = CreateStoppableCondition();

            var queryRequest = new QueryRequest
            {
                Condition = $"{integrationPointCondition} AND {stoppableCondition}"
            };

            return _relativityObjectManager.Query<JobHistory>(queryRequest);
        }

        private void UpdateJobHistoryStatusField(int jobHistoryID, ChoiceRef status, DateTime jobEndTime)
        {
            var fieldValues = new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        Guid = JobHistoryFieldGuids.JobStatusGuid
                    },
                    Value = status
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        Guid = JobHistoryFieldGuids.EndTimeUTCGuid
                    },
                    Value = jobEndTime
                }
            };

            _relativityObjectManager.Update(jobHistoryID, fieldValues);
        }

        private void UpdateIntegrationPointFields(int integrationPointID, DateTime jobEndTime)
        {
            var fieldValues = new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        Guid = IntegrationPointFieldGuids.LastRuntimeUTCGuid
                    },
                    Value = jobEndTime
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        Guid = IntegrationPointFieldGuids.HasErrorsGuid
                    },
                    Value = true
                }
            };

            _relativityObjectManager.Update(integrationPointID, fieldValues);
        }

        private string CreateIntegrationPointCondition(int integrationPointArtifactId)
        {
            return $"('{JobHistoryFields.IntegrationPoint}' INTERSECTS MULTIOBJECT [{integrationPointArtifactId}])";
        }

        private string CreateStoppableCondition()
        {
            return $"('{JobHistoryFields.JobStatus}' IN CHOICE [" +
                $"{JobStatusChoices.JobHistoryPendingGuid}, " +
                $"{JobStatusChoices.JobHistoryProcessingGuid}, " +
                $"{JobStatusChoices.JobHistoryValidatingGuid}])";
        }
    }
}
