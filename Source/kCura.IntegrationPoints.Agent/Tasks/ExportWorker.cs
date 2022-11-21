using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public class ExportWorker : SyncWorker
    {
        private readonly ExportProcessRunner _exportProcessRunner;
        private readonly IDataTransferLocationService _dataTransferLocationService;
        private readonly IAPILog _logger;

        public ExportWorker(
            ICaseServiceContext caseServiceContext,
            IHelper helper,
            IDataProviderFactory dataProviderFactory,
            ISerializer serializer,
            ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
            IJobHistoryService jobHistoryService,
            JobHistoryErrorServiceProvider jobHistoryErrorServiceProvider,
            IJobManager jobManager,
            IEnumerable<IBatchStatus> statuses,
            IJobStatisticsService statisticsService,
            ExportProcessRunner exportProcessRunner,
            IManagerFactory managerFactory,
            IJobService jobService,
            IDataTransferLocationService dataTransferLocationService,
            IProviderTypeService providerTypeService,
            IIntegrationPointService integrationPointService,
            IDiagnosticLog diagnosticLog)
            : base(
                caseServiceContext,
                helper,
                dataProviderFactory,
                serializer,
                appDomainRdoSynchronizerFactoryFactory,
                jobHistoryService,
                jobHistoryErrorServiceProvider.JobHistoryErrorService,
                jobManager,
                statuses,
                statisticsService,
                managerFactory,
                jobService,
                providerTypeService,
                integrationPointService,
                diagnosticLog)
        {
            _exportProcessRunner = exportProcessRunner;
            _dataTransferLocationService = dataTransferLocationService;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportWorker>();
        }

        protected override IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo,
            string configuration, Job job)
        {
            var providerGuid = new Guid(destinationProviderRdo.Identifier);
            IDataSynchronizer sourceProvider = AppDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(providerGuid, configuration);
            return sourceProvider;
        }

        protected override void ExecuteImport(IEnumerable<FieldMap> fieldMap, DataSourceProviderConfiguration configuration,
            string destinationConfiguration, List<string> entryIDs, SourceProvider sourceProviderRdo, DestinationProvider destinationProvider, Job job)
        {
            LogExecuteImportStart(job);

            ExportUsingSavedSearchSettings sourceSettings = DeserializeSourceSettings(configuration.Configuration, job);
            ImportSettings destinationSettings = DeserializeDestinationSettings(destinationConfiguration, job);

            PrepareDestinationLocation(sourceSettings);

            _exportProcessRunner.StartWith(sourceSettings, fieldMap, destinationSettings.ArtifactTypeId, job);
            LogExecuteImportSuccessfulEnd(job);
        }

        private ExportUsingSavedSearchSettings DeserializeSourceSettings(string sourceConfiguration, Job job)
        {
            try
            {
                return JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(sourceConfiguration);
            }
            catch (Exception e)
            {
                LogDeserializationOfSourceSettingsError(job, e);
                throw;
            }
        }

        private ImportSettings DeserializeDestinationSettings(string destinationConfiguration, Job job)
        {
            try
            {
                return JsonConvert.DeserializeObject<ImportSettings>(destinationConfiguration);
            }
            catch (Exception e)
            {
                LogDeserializationOfDestinationSettingsError(job, e);
                throw;
            }
        }

        private void PrepareDestinationLocation(ExportUsingSavedSearchSettings settings)
        {
            try
            {
                settings.Fileshare = _dataTransferLocationService.VerifyAndPrepare(CaseServiceContext.WorkspaceID,
                    settings.Fileshare,
                    Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid);
            }
            catch (Exception e)
            {
                LogDataTransferLocationPreparationError(e);
                throw;
            }
        }

        #region Logging

        private void LogDeserializationOfSourceSettingsError(Job job, Exception e)
        {
            _logger.LogError(e, "Failed to deserialize source settings for job {JobId}.", job.JobId);
        }

        private void LogDeserializationOfDestinationSettingsError(Job job, Exception e)
        {
            _logger.LogError(e, "Failed to deserialize destination settings for job {JobId}.", job.JobId);
        }

        private void LogDataTransferLocationPreparationError(Exception e)
        {
            _logger.LogError(e, "Failed to create transfer location's directory structure");
        }

        private void LogExecuteImportSuccessfulEnd(Job job)
        {
            _logger.LogInformation("Successfully finished execution of import in Export Worker for: {JobId}.", job.JobId);
        }

        private void LogExecuteImportStart(Job job)
        {
            _logger.LogInformation("Starting execution of import in Export Worker for: {JobId}.", job.JobId);
        }

        #endregion
    }
}
