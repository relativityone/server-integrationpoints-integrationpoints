using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.QueryOptions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Utils;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public class JobHistoryService : IJobHistoryService
    {
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly IFederatedInstanceManager _federatedInstanceManager;
        private readonly IWorkspaceManager _workspaceManager;
        private readonly IAPILog _logger;
        private readonly ISerializer _serializer;

        public JobHistoryService(
            IRelativityObjectManager relativityObjectManager,
            IFederatedInstanceManager federatedInstanceManager,
            IWorkspaceManager workspaceManager,
            IAPILog logger,
            ISerializer serializer)
        {
            _relativityObjectManager = relativityObjectManager;
            _federatedInstanceManager = federatedInstanceManager;
            _workspaceManager = workspaceManager;
            _logger = logger.ForContext<JobHistoryService>();
            _serializer = serializer;
        }

        public Data.JobHistory GetRdo(Guid batchInstance)
        {
            JobHistoryQueryOptions queryOptions = JobHistoryQueryOptions.All();
            return GetRdo(batchInstance, queryOptions);
        }

        public Data.JobHistory GetRdoWithoutDocuments(Guid batchInstance)
        {
            JobHistoryQueryOptions queryOptions = JobHistoryQueryOptions
                .All()
                .Except(JobHistoryFieldGuids.Documents);
            return GetRdo(batchInstance, queryOptions);
        }

        public Data.JobHistory GetRdoWithoutDocuments(int artifactId)
        {
            JobHistoryQueryOptions queryOptions = JobHistoryQueryOptions
                .All()
                .Except(JobHistoryFieldGuids.Documents);

            return GetRdo(artifactId, queryOptions);
        }

        public IList<Data.JobHistory> GetJobHistory(IList<int> jobHistoryArtifactIds)
        {
            _logger.LogInformation("Getting JobHistory for [{jobHistoryArtifactIds}]",
                string.Join(",", jobHistoryArtifactIds?.ToList() ?? new List<int>()));

            var request = new QueryRequest
            {
                Condition = $"'{ArtifactQueryFieldNames.ArtifactID}' in [{string.Join(",", jobHistoryArtifactIds.ToList())}]",
                Fields = RDOConverter.GetFieldList<Data.JobHistory>()
            };

            IList<Data.JobHistory> jobHistories = _relativityObjectManager.Query<Data.JobHistory>(request);
            return jobHistories;
        }

        public Data.JobHistory GetOrCreateScheduledRunHistoryRdo(
            IntegrationPointDto integrationPointDto,
            Guid batchInstance,
            DateTime? startTimeUtc)
        {
            Data.JobHistory jobHistory = null;

            try
            {
                jobHistory = GetRdo(batchInstance);
            }
            catch (Exception e)
            {
                LogHistoryNotFoundError(integrationPointDto, e);
                // ignored
            }

            if (jobHistory == null)
            {
                _logger.LogInformation("JobHistory {batchInstance} doesn't exist. Create new...", batchInstance);
                jobHistory = CreateRdo(integrationPointDto, batchInstance, JobTypeChoices.JobHistoryScheduledRun, startTimeUtc);
                integrationPointDto.JobHistory.Add(jobHistory.ArtifactId);
            }

            _logger.LogInformation("Read JobHistory: {jobHistoryDetails}", jobHistory.Stringify());

            return jobHistory;
        }

        public Data.JobHistory CreateRdo(
            IntegrationPointDto integrationPointDto,
            Guid batchInstance,
            ChoiceRef jobType,
            DateTime? startTimeUtc)
        {
            Data.JobHistory jobHistory = null;

            try
            {
                jobHistory = GetRdo(batchInstance);
            }
            catch (Exception e)
            {
                LogCreatingHistoryRdoError(e);
                // ignored
            }

            if (jobHistory != null)
            {
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
                ItemsWithErrors = 0,
                Overwrite = integrationPointDto.SelectedOverwrite,
                JobID = null
            };

            ImportSettings importSettings = _serializer.Deserialize<ImportSettings>(integrationPointDto.DestinationConfiguration);

            try
            {
                WorkspaceDTO workspaceDto = _workspaceManager.RetrieveWorkspace(importSettings.CaseArtifactId);
                if (workspaceDto != null)
                {
                    jobHistory.DestinationWorkspace = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(workspaceDto.Name, importSettings.CaseArtifactId);
                }
            }
            catch (Exception ex)
            {
                jobHistory.DestinationWorkspace = "[Unable to retrieve workspace name]";
                LogGettingWorkspaceNameError(ex);
            }

            FederatedInstanceDto federatedInstanceDto = _federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(importSettings.FederatedInstanceArtifactId);
            if (federatedInstanceDto != null)
            {
                jobHistory.DestinationInstance = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(federatedInstanceDto.Name, federatedInstanceDto.ArtifactId);
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

        public void UpdateRdo(Data.JobHistory jobHistory)
        {
            JobHistoryQueryOptions queryOptions = JobHistoryQueryOptions.All();
            UpdateRdo(jobHistory, queryOptions);
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

        public IList<Data.JobHistory> GetAll()
        {
            return _relativityObjectManager.Query<Data.JobHistory>(new QueryRequest()
            {
                Fields = RDOConverter.GetFieldList<Data.JobHistory>()
            });
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

        private Data.JobHistory GetRdo(
            Guid batchInstance,
            JobHistoryQueryOptions queryOptions)
        {
            var request = new QueryRequest
            {
                Condition = $"'{JobHistoryFields.BatchInstance}' == '{batchInstance}'",
                Fields = MapToFieldRefs(queryOptions?.FieldGuids)
            };

            IList<Data.JobHistory> jobHistories = _relativityObjectManager.Query<Data.JobHistory>(request);
            if (jobHistories.Count > 1)
            {
                LogMoreThanOneHistoryInstanceWarning(batchInstance);
            }
            Data.JobHistory jobHistory = jobHistories.SingleOrDefault(); //there should only be one!

            _logger.LogInformation("Read JobHistory for BatchInstanceId: {batchInstanceId}. JobHistory: {@jobHistoryDetails}",
                batchInstance, jobHistory.Stringify());

            return jobHistory;
        }

        private Data.JobHistory GetRdo(
            int artifactId,
            JobHistoryQueryOptions queryOptions)
        {
            var request = new QueryRequest
            {
                Condition = $"'Artifact ID' == '{artifactId}'",
                Fields = MapToFieldRefs(queryOptions?.FieldGuids)
            };

            IList<Data.JobHistory> jobHistories = _relativityObjectManager.Query<Data.JobHistory>(request);

            if (jobHistories.Count > 1)
            {
                LogMoreThanOneHistoryInstanceWarning(artifactId);
            }

            Data.JobHistory jobHistory = jobHistories.SingleOrDefault();

            _logger.LogInformation("Read JobHistory Artifact ID: {artifactId}. JobHistory: {@jobHistoryDetails}",
                artifactId, jobHistory?.Stringify());

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

        #region Logging

        private void LogGettingWorkspaceNameError(Exception exception)
        {
            _logger.LogWarning(exception, "Unable to get workspace name from destination workspace");
        }

        private void LogMoreThanOneHistoryInstanceWarning(Guid batchInstance)
        {
            _logger.LogWarning("More than one job history instance found for BatchInstance: {BatchInstance}", batchInstance.ToString());
        }

        private void LogMoreThanOneHistoryInstanceWarning(int jobHistoryArtifactId)
        {
            _logger.LogWarning("More than one job history instance found for Artifact ID: {ArtifactID}", jobHistoryArtifactId);
        }

        private void LogHistoryNotFoundError(IntegrationPointDto integrationPointDto, Exception e)
        {
            _logger.LogError(e, "Job history for Integration Point {IntegrationPointId} not found.", integrationPointDto.ArtifactId);
        }

        private void LogCreatingHistoryRdoError(Exception e)
        {
            _logger.LogError(e, "Failed to create History RDO.");
        }

        #endregion
    }
}
