using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportWorker : SyncWorker
	{
		#region Constructor

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
			JobStatisticsService statisticsService,
			ExportProcessRunner exportProcessRunner,
			IManagerFactory managerFactory,
			IContextContainerFactory contextContainerFactory,
			IJobService jobService,
			IDataTransferLocationService dataTransferLocationService
		) : base(
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
			contextContainerFactory,
			jobService)
		{
			_exportProcessRunner = exportProcessRunner;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportWorker>();
			_dataTransferLocationService = dataTransferLocationService;
		}

		#endregion //Constructor

		#region Fields

		private readonly ExportProcessRunner _exportProcessRunner;
		private readonly IAPILog _logger;
		private readonly IDataTransferLocationService _dataTransferLocationService;

		#endregion //Fields

		#region Methods

		protected override IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo,
			string configuration, Job job)
		{
			var providerGuid = new Guid(destinationProviderRdo.Identifier);

			var sourceProvider = AppDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(providerGuid, configuration);
			return sourceProvider;
		}

		protected override void ExecuteImport(IEnumerable<FieldMap> fieldMap, string sourceConfiguration,
			string destinationConfiguration, List<string> entryIDs,
			SourceProvider sourceProviderRdo, DestinationProvider destinationProvider, Job job)
		{
		    LogExecuteImportStart(job);

            var sourceSettings = DeserializeSourceSettings(sourceConfiguration, job);
			var destinationSettings = DeserializeDestinationSettings(destinationConfiguration, job);

			PrepareDestinationLocation(sourceSettings);

			_exportProcessRunner.StartWith(sourceSettings, fieldMap, destinationSettings.ArtifactTypeId, job);
		    LogExecuteImportSuccesfulEnd(job);

		}

	    private ExportUsingSavedSearchSettings DeserializeSourceSettings(string sourceConfiguration, Job job)
		{
			try
			{
				return JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(sourceConfiguration);
			}
			catch (Exception e)
			{
				LogDeserializationOfSourceSettingsError(job, sourceConfiguration, e);
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
				LogDeserializationOfDestinationSettingsError(job, destinationConfiguration, e);
				throw;
			}
		}

		private void PrepareDestinationLocation(ExportUsingSavedSearchSettings settings)
		{
			try
			{
			    if (_dataTransferLocationService.IsEddsPath(settings.Fileshare))
			    {
			        settings.Fileshare = _dataTransferLocationService.VerifyAndPrepare(CaseServiceContext.WorkspaceID,
			            settings.Fileshare,
			            Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid);
			    }
			}
			catch (Exception e)
			{
				LogDataTransferLocationPreparationError(settings?.Fileshare, e);
				throw;
			}
		}

		#endregion //Methods

		#region Logging

		private void LogDeserializationOfSourceSettingsError(Job job, string sourceSettings, Exception e)
		{
			_logger.LogError(e, "Failed to deserialize source settings ({SourceSettings}) for job {JobId}.", sourceSettings, job.JobId);
		}

		private void LogDeserializationOfDestinationSettingsError(Job job, string destinationSettings, Exception e)
		{
			_logger.LogError(e, "Failed to deserialize destination settings ({DestinationSettings}) for job {JobId}.", destinationSettings, job.JobId);
		}

		private void LogDataTransferLocationPreparationError(string path, Exception e)
		{
			_logger.LogError(e, "Failed to create transfer location'sdirectory structure", path);
		}

	    private void LogExecuteImportSuccesfulEnd(Job job)
	    {
	        _logger.LogInformation("Succesfully finished execution of import in Export Worker for: {JobId}.", job.JobId);
	    }

	    private void LogExecuteImportStart(Job job)
	    {
	        _logger.LogInformation("Starting execution of import in Export Worker for: {JobId}.", job.JobId);
	    }

        #endregion
    }
}