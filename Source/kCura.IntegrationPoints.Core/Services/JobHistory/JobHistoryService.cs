using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.QueryOptions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Utils;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public class JobHistoryService : IJobHistoryService
    {
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly IWorkspaceManager _workspaceManager;
        private readonly IAPILog _logger;

        public JobHistoryService(
            IRelativityObjectManager relativityObjectManager,
            IWorkspaceManager workspaceManager,
            IAPILog logger)
        {
            _relativityObjectManager = relativityObjectManager;
            _workspaceManager = workspaceManager;
            _logger = logger.ForContext<JobHistoryService>();
        }

        public Data.JobHistory GetRdoWithoutDocuments(Guid batchInstance)
        {
            JobHistoryQueryOptions queryOptions = JobHistoryQueryOptions
                .All()
                .Except(JobHistoryFieldGuids.Documents);

            return GetRdo(GetBatchInstanceQueryCondition(batchInstance), queryOptions);
        }

        public Data.JobHistory GetRdoWithoutDocuments(int artifactId)
        {
            JobHistoryQueryOptions queryOptions = JobHistoryQueryOptions
                .All()
                .Except(JobHistoryFieldGuids.Documents);

            return GetRdo(GetArtifactIdQueryCondition(artifactId), queryOptions);
        }

        public Data.JobHistory GetOrCreateScheduledRunHistoryRdo(IntegrationPointDto integrationPointDto, Guid batchInstance, DateTime? startTimeUtc)
        {
            Data.JobHistory jobHistory = GetRdoWithoutDocuments(batchInstance);
            if (jobHistory == null)
            {
                _logger.LogInformation("JobHistory {batchInstance} doesn't exist. Create new...", batchInstance);
                jobHistory = CreateRdo(integrationPointDto, batchInstance, JobTypeChoices.JobHistoryScheduledRun, startTimeUtc);
                integrationPointDto.JobHistory.Add(jobHistory.ArtifactId);
            }

            _logger.LogInformation("Read JobHistory: {jobHistoryDetails}", jobHistory.Stringify());
            return jobHistory;
        }

        public Data.JobHistory CreateRdo(IntegrationPointDto integrationPointDto, Guid batchInstance, ChoiceRef jobType, DateTime? startTimeUtc)
        {
            Data.JobHistory jobHistory = GetRdoWithoutDocuments(batchInstance);
            if (jobHistory != null)
            {
                _logger.LogWarning("JobHistory already exists. Withdrawn from creating the new one: {jobHistoryDetails}", jobHistory.Stringify());
                return jobHistory;
            }

            jobHistory = new Data.JobHistory
            {
                Name = integrationPointDto.Name,
                IntegrationPoint = new[] { integrationPointDto.ArtifactId },
                BatchInstance = batchInstance.ToString(),
                JobType = jobType,
                JobStatus = JobStatusChoices.JobHistoryPending,
                ItemsTransferred = 0,
                ItemsRead = 0,
                ItemsWithErrors = 0,
                Overwrite = integrationPointDto.SelectedOverwrite,
                JobID = null
            };

            int workspaceId = integrationPointDto.DestinationConfiguration.CaseArtifactId;
            try
            {
                WorkspaceDTO workspaceDto = _workspaceManager.RetrieveWorkspace(workspaceId);
                if (workspaceDto != null)
                {
                    jobHistory.DestinationWorkspace = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(workspaceDto.Name, workspaceId);
                }
            }
            catch (Exception ex)
            {
                jobHistory.DestinationWorkspace = "[Unable to retrieve workspace name]";
                _logger.LogWarning(ex, "Unable to get workspace name from destination workspace");
            }

            if (startTimeUtc.HasValue)
            {
                jobHistory.StartTimeUTC = startTimeUtc.Value;
            }

            int artifactId = _relativityObjectManager.Create(jobHistory);
            jobHistory.ArtifactId = artifactId;

            _logger.LogInformation("Created JobHistory: {jobHistoryDetails}", jobHistory.Stringify());
            return jobHistory;
        }

        public void UpdateRdoWithoutDocuments(Data.JobHistory jobHistory)
        {
            JobHistoryQueryOptions queryOptions = JobHistoryQueryOptions
                .All()
                .Except(JobHistoryFieldGuids.Documents);
            UpdateRdo(jobHistory, queryOptions);
        }

        public void DeleteRdo(int jobHistoryId)
        {
            _relativityObjectManager.Delete(jobHistoryId);
        }

        private void UpdateRdo(
            Data.JobHistory jobHistory,
            JobHistoryQueryOptions queryOptions)
        {
            _logger.LogInformation("Updating JobHistory {jobHistoryId}", jobHistory.ArtifactId);

            if (queryOptions.ContainsAll())
            {
                _relativityObjectManager.Update(jobHistory);
                return;
            }

            List<FieldRefValuePair> fieldValues = MapToFieldValues(queryOptions.FieldGuids, jobHistory)
                .ToList();

            _relativityObjectManager.Update(jobHistory.ArtifactId, fieldValues);
        }

        private Data.JobHistory GetRdo(string queryCondition, JobHistoryQueryOptions queryOptions)
        {
            var request = new QueryRequest
            {
                Condition = queryCondition,
                Fields = MapToFieldRefs(queryOptions?.FieldGuids)
            };

            Stopwatch sw = Stopwatch.StartNew();
            List<Data.JobHistory> jobHistories = _relativityObjectManager.Query<Data.JobHistory>(request);
            sw.Stop();

            _logger.LogInformation("JobHistoryService JobHistory Query to ObjectManager with [{queryCondition}] elapsed time: {jobHistoryQueryElapsedTimeMs} ms", queryCondition, sw.ElapsedMilliseconds);

            if (jobHistories.Count > 1)
            {
                _logger.LogWarning("More than one job history instance found for query condition: {queryCondition}", queryCondition);
            }

            Data.JobHistory jobHistory = jobHistories.OrderBy(x => x.ArtifactId).FirstOrDefault(); // there should only be one, but in case of multiple records we are resilient for many job histories

            if (jobHistory == null)
            {
                _logger.LogWarning("No job history instance found for query condition: {queryCondition}", queryCondition);
            }

            return jobHistory;
        }

        private IEnumerable<FieldRef> MapToFieldRefs(Guid[] rdoFieldGuids)
        {
            IEnumerable<FieldRef> fieldRefs = RDOConverter.GetFieldList<Data.JobHistory>();
            return rdoFieldGuids == null
                ? fieldRefs
                : fieldRefs.Where(fr => rdoFieldGuids.Contains(fr.Guid.Value));
        }

        private IEnumerable<FieldRefValuePair> MapToFieldValues(
            Guid[] rdoFieldGuids,
            Data.JobHistory jobHistory)
        {
            IEnumerable<FieldRefValuePair> fieldValues = jobHistory.ToFieldValues();
            return rdoFieldGuids == null
                ? fieldValues
                : fieldValues.Where(fv => rdoFieldGuids.Contains(fv.Field.Guid.Value));
        }

        private string GetBatchInstanceQueryCondition(Guid batchInstance) => $"'{JobHistoryFields.BatchInstance}' == '{batchInstance}'";

        private string GetArtifactIdQueryCondition(int artifactId) => $"'Artifact ID' == '{artifactId}'";
    }
}
