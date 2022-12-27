using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public class ExportManager : SyncManager
    {
        private readonly IExportInitProcessService _exportInitProcessService;
        private readonly IAPILog _logger;

        public override int BatchSize
        {
            get
            {
                //Currently Export Shared library (kCura.WinEDDS) is making usage of batching internalLy
                //so for now we need to create only one worker job
                return int.MaxValue;
            }
        }

        public ExportManager(
            ICaseServiceContext caseServiceContext,
            IDataProviderFactory providerFactory,
            IJobManager jobManager,
            IJobService jobService,
            IHelper helper,
            IIntegrationPointService integrationPointService,
            ISerializer serializer, IGuidService guidService,
            IJobHistoryService jobHistoryService,
            IJobHistoryErrorService jobHistoryErrorService,
            IScheduleRuleFactory scheduleRuleFactory,
            IManagerFactory managerFactory,
            IEnumerable<IBatchStatus> batchStatuses,
            IExportInitProcessService exportInitProcessService,
            IAgentValidator agentValidator,
            IDiagnosticLog diagnosticLog)
            : base(
                caseServiceContext, providerFactory, jobManager, jobService, helper, integrationPointService, serializer, guidService, jobHistoryService, jobHistoryErrorService,
                scheduleRuleFactory, managerFactory, batchStatuses, agentValidator, diagnosticLog)
        {
            _exportInitProcessService = exportInitProcessService;
            _logger = Helper.GetLoggerFactory().GetLogger().ForContext<ExportManager>();
        }

        protected override TaskType GetTaskType()
        {
            return TaskType.ExportWorker;
        }

        /// <summary>
        ///     This method returns record (batch) ids that should be processed by ExportWorker class
        /// </summary>
        /// <param name="job">Details of the export job</param>
        /// <returns>List of batch ids to be processed</returns>
        public override IEnumerable<string> GetUnbatchedIDs(Job job)
        {
            //Currently Export Shared library (kCura.WinEDDS) is making usage of batching internalLy
            //so for now we need to create only one worker job
            LogGettingUnbatchedIDs(job);
            return Enumerable.Empty<string>(); // not using "yield break" to prevent lazy evaluation of IEnumerable
        }

        public override long BatchTask(Job job, IEnumerable<string> batchIDs)
        {
            LogBatchTaskStart(job, batchIDs);
            IntegrationPointDto integrationPoint = IntegrationPointService.Read(job.RelatedObjectArtifactID);

            if (integrationPoint == null)
            {
                LogUnableToRetrieveIntegrationPoint(job);
                throw new Exception("Failed to retrieve corresponding Integration Point.");
            }

            ExportUsingSavedSearchSettings sourceSettings = Serializer.Deserialize<ExportUsingSavedSearchSettings>(integrationPoint.SourceConfiguration);
            DestinationConfiguration destinationConfiguration = JsonConvert.DeserializeObject<DestinationConfiguration>(integrationPoint.DestinationConfiguration);

            long totalCount = GetTotalExportItemsCount(sourceSettings, destinationConfiguration, job);

            if (totalCount > 0)
            {
                CreateBatchJob(job, new List<string>(), -1);
            }

            LogBatchTaskSuccesfulEnd(job, totalCount);
            return totalCount;
        }

        private long GetTotalExportItemsCount(ExportUsingSavedSearchSettings settings, DestinationConfiguration destinationConfiguration, Job job)
        {
            try
            {
                LogGetTotalExportItemsCountStart(job);
                long count = _exportInitProcessService.CalculateDocumentCountToTransfer(settings, destinationConfiguration.ArtifactTypeId);
                LogGetTotalExportItemsCountSuccesfulEnd(job, count);
                return count;
            }
            catch (Exception ex)
            {
                LogRetrievingExportItemsCountError(job, ex);
                throw;
            }
        }

        #region Logging

        private void LogRetrievingExportItemsCountError(Job job, Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve total export items count for job {JobId}.", job.JobId);
        }

        private void LogUnableToRetrieveIntegrationPoint(Job job)
        {
            _logger.LogError("Failed to retrieve Integration Point object for job {JobId}.", job.JobId);
        }

        private void LogGettingUnbatchedIDs(Job job)
        {
            _logger.LogInformation("Getting unbatched IDs in Export Worker for job {JobId}.", job.JobId);
        }

        private void LogBatchTaskStart(Job job, IEnumerable<string> batchIDs)
        {
            _logger.LogInformation("Started batch task in Export Manager for job: {JobId}, batchIds: {batchIDs}",
                job.JobId,
                batchIDs);
        }

        private void LogBatchTaskSuccesfulEnd(Job job, long totalCount)
        {
            _logger.LogInformation("Finished batch task in Export Manager for job: {JobId}, totalCount: {totalCount}",
                job.JobId, totalCount);
        }

        private void LogGetTotalExportItemsCountStart(Job job)
        {
            _logger.LogInformation("Trying to get total export items count for job: {JobId}", job.JobId);
        }

        private void LogGetTotalExportItemsCountSuccesfulEnd(Job job, long count)
        {
            _logger.LogInformation("Retrieved total export items count for job: {JobId}, count: {count}", job.JobId, count);
        }

        #endregion
    }
}
