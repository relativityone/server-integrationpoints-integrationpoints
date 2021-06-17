﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.RelativitySync.Models;
using kCura.IntegrationPoints.RelativitySync.Utils;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.SyncConfiguration;
using Relativity.Sync.SyncConfiguration.FieldsMapping;
using Relativity.Sync.SyncConfiguration.Options;

using SyncFieldMap = Relativity.Sync.Storage.FieldMap;

namespace kCura.IntegrationPoints.RelativitySync
{
	public sealed class IntegrationPointToSyncConverter
	{
		private readonly ISerializer _serializer;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobHistorySyncService _jobHistorySyncService;
		private readonly IAPILog _logger;

		public IntegrationPointToSyncConverter(ISerializer serializer, IJobHistoryService jobHistoryService, 
			IJobHistorySyncService jobHistorySyncService, IAPILog logger)
		{
			_serializer = serializer;
			_jobHistoryService = jobHistoryService;
			_jobHistorySyncService = jobHistorySyncService;
			_logger = logger;
		}

		public async Task<int> CreateSyncConfigurationAsync(IExtendedJob job, ISyncServiceManager servicesMgr)
		{
			SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(job.IntegrationPointModel.SourceConfiguration);
			ImportSettings importSettings = _serializer.Deserialize<ImportSettings>(job.IntegrationPointModel.DestinationConfiguration);
			FolderConf folderConf = _serializer.Deserialize<FolderConf>(job.IntegrationPointModel.DestinationConfiguration);

			ISyncContext syncContext = new SyncContext(job.WorkspaceId, sourceConfiguration.TargetWorkspaceArtifactId, job.JobHistoryId);

			ISyncConfigurationBuilder builder = new SyncConfigurationBuilder(syncContext, servicesMgr);

			if (importSettings.ImageImport)
			{
				return await CreateImageSyncConfigurationAsync(builder,
					job, sourceConfiguration, importSettings).ConfigureAwait(false);
			}
			else
			{
				return await CreateDocumentSyncConfigurationAsync(builder,
					job, sourceConfiguration, importSettings, folderConf).ConfigureAwait(false);
			}
		}

		private async Task<int> CreateImageSyncConfigurationAsync(ISyncConfigurationBuilder builder, IExtendedJob job,
			SourceConfiguration sourceConfiguration, ImportSettings importSettings)
		{
			IEnumerable<int> productionImagePrecedenceIds = importSettings.ProductionPrecedence == "1" ?
				importSettings.ImagePrecedence.Select(x => int.Parse(x.ArtifactID)) :
				Enumerable.Empty<int>();

			IImageSyncConfigurationBuilder syncConfigurationRoot = builder
				.ConfigureImageSync(
					new ImageSyncOptions(
						DataSourceType.SavedSearch, sourceConfiguration.SavedSearchArtifactId,
						DestinationLocationType.Folder, importSettings.DestinationFolderArtifactId)
					{
						CopyImagesMode = importSettings.ImportNativeFileCopyMode.ToSyncImageMode()
					})
				.ProductionImagePrecedence(
					new ProductionImagePrecedenceOptions(
						productionImagePrecedenceIds,
						importSettings.IncludeOriginalImages))
				.EmailNotifications(
					GetEmailOptions(job))
				.OverwriteMode(
					new OverwriteOptions(
						importSettings.ImportOverwriteMode.ToSyncImportOverwriteMode())
					{
						FieldsOverlayBehavior = importSettings.ImportOverlayBehavior.ToSyncFieldOverlayBehavior()
					})
				.CreateSavedSearch(
					new CreateSavedSearchOptions(
						importSettings.CreateSavedSearchForTagging));

			if (IsRetryingErrors(job.Job))
			{
				RelativityObject jobToRetry = await _jobHistorySyncService.GetLastJobHistoryWithErrorsAsync(
					sourceConfiguration.SourceWorkspaceArtifactId, job.IntegrationPointId).ConfigureAwait(false);

				syncConfigurationRoot.IsRetry(new RetryOptions(jobToRetry));
			}

			return await syncConfigurationRoot.SaveAsync().ConfigureAwait(false);
		}

		private async Task<int> CreateDocumentSyncConfigurationAsync(ISyncConfigurationBuilder builder, IExtendedJob job,
			SourceConfiguration sourceConfiguration, ImportSettings importSettings, FolderConf folderConf)
		{
			IDocumentSyncConfigurationBuilder syncConfigurationRoot = builder
				.ConfigureDocumentSync(
					new DocumentSyncOptions(
						sourceConfiguration.SavedSearchArtifactId,
						importSettings.DestinationFolderArtifactId)
					{
						CopyNativesMode = importSettings.ImportNativeFileCopyMode.ToSyncNativeMode()
					})
				.WithFieldsMapping(mappingBuilder => PrepareFieldsMappingAction(
					job.IntegrationPointModel.FieldMappings,mappingBuilder))
				.DestinationFolderStructure(
					GetFolderStructureOptions(folderConf, importSettings))
				.EmailNotifications(
					GetEmailOptions(job))
				.OverwriteMode(
					new OverwriteOptions(
						importSettings.ImportOverwriteMode.ToSyncImportOverwriteMode())
					{
						FieldsOverlayBehavior = importSettings.ImportOverlayBehavior.ToSyncFieldOverlayBehavior()
					})
				.CreateSavedSearch(
					new CreateSavedSearchOptions(
						importSettings.CreateSavedSearchForTagging));

			if (IsRetryingErrors(job.Job))
			{
				RelativityObject jobToRetry = await _jobHistorySyncService.GetLastJobHistoryWithErrorsAsync(
					sourceConfiguration.SourceWorkspaceArtifactId, job.IntegrationPointId).ConfigureAwait(false);

				syncConfigurationRoot.IsRetry(new RetryOptions(jobToRetry));
			}

			return await syncConfigurationRoot.SaveAsync().ConfigureAwait(false);
		}

		private void PrepareFieldsMappingAction(string integrationPointsFieldsMapping, IFieldsMappingBuilder mappingBuilder)
		{
			List<FieldMap> fieldsMapping = FieldMapHelper.FixedSyncMapping(integrationPointsFieldsMapping, _serializer, _logger);

			SyncFieldMap identifier = fieldsMapping.FirstOrDefault(x => x.FieldMapType == FieldMapType.Identifier);
			if (identifier != null)
			{
				mappingBuilder.WithIdentifier();
			}

			foreach (FieldMap fieldsMap in fieldsMapping.Where(x => x.FieldMapType == FieldMapType.None))
			{
				mappingBuilder.WithField(fieldsMap.SourceField.FieldIdentifier, fieldsMap.DestinationField.FieldIdentifier);
			}
		}

		private DestinationFolderStructureOptions GetFolderStructureOptions(FolderConf folderConf, ImportSettings settings)
		{
			if (folderConf.UseFolderPathInformation)
			{
				DestinationFolderStructureOptions folderOptions = DestinationFolderStructureOptions.ReadFromField(folderConf.FolderPathSourceField);
				folderOptions.MoveExistingDocuments = settings.MoveExistingDocuments;

				return folderOptions;
			}

			if (folderConf.UseDynamicFolderPath)
			{
				DestinationFolderStructureOptions folderOptions = DestinationFolderStructureOptions.RetainFolderStructureFromSourceWorkspace();
				folderOptions.MoveExistingDocuments = settings.MoveExistingDocuments;

				return folderOptions;
			}

			return DestinationFolderStructureOptions.None();
		}

		private EmailNotificationsOptions GetEmailOptions(IExtendedJob job)
		{
			if(job.IntegrationPointModel.EmailNotificationRecipients == null)
			{
				return new EmailNotificationsOptions(new List<string>());
			}

			List<string> emailsList = job.IntegrationPointModel
				.EmailNotificationRecipients
				.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
				.ToList();

			return new EmailNotificationsOptions(emailsList);
		}

		private bool IsRetryingErrors(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			JobHistory jobHistory = _jobHistoryService.GetRdo(taskParameters.BatchInstance);

			if (jobHistory == null)
			{
				// this means that job is scheduled, so it's not retrying errors 
				return false;
			}

			return jobHistory.JobType.EqualsToChoice(JobTypeChoices.JobHistoryRetryErrors);
		}
	}
}