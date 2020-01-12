﻿using System;
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
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
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
			IJobService jobService,
			IDataTransferLocationService dataTransferLocationService,
			IProcessingSourceLocationService processingSourceLocationService,
			IProviderTypeService providerTypeService,
			IIntegrationPointRepository integrationPointRepository)
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
				integrationPointRepository)
		{
			_exportProcessRunner = exportProcessRunner;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportWorker>();
			_dataTransferLocationService = dataTransferLocationService;
			_processingSourceLocationService = processingSourceLocationService;
		}

		#endregion //Constructor

		#region Fields

		private readonly ExportProcessRunner _exportProcessRunner;
		private readonly IAPILog _logger;
		private readonly IDataTransferLocationService _dataTransferLocationService;
		private readonly IProcessingSourceLocationService _processingSourceLocationService;

		#endregion //Fields

		#region Methods

		protected override IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo,
			string configuration, Job job)
		{
			var providerGuid = new Guid(destinationProviderRdo.Identifier);

			var sourceProvider = AppDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(providerGuid, configuration);
			return sourceProvider;
		}

		protected override void ExecuteImport(IEnumerable<FieldMap> fieldMap, DataSourceProviderConfiguration configuration, string destinationConfiguration, List<string> entryIDs, SourceProvider sourceProviderRdo, DestinationProvider destinationProvider, Job job)
		{
			LogExecuteImportStart(job);

			var sourceSettings = DeserializeSourceSettings(configuration.Configuration, job);
			var destinationSettings = DeserializeDestinationSettings(destinationConfiguration, job);

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
				if (PathIsProcessingSourceLocation(settings))
				{
					return;
				}

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

		private bool PathIsProcessingSourceLocation(ExportUsingSavedSearchSettings settings)
		{
			return _processingSourceLocationService.IsEnabled() &&
				   _processingSourceLocationService.IsProcessingSourceLocation(settings.Fileshare,
					   CaseServiceContext.WorkspaceID);
		}

		#endregion //Methods

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