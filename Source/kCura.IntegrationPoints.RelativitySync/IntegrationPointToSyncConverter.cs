using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FieldMap = Relativity.IntegrationPoints.FieldsMapping.Models.FieldMap;
using SyncFieldMap = Relativity.Sync.Storage.FieldMap;

namespace kCura.IntegrationPoints.RelativitySync
{
	public sealed class IntegrationPointToSyncConverter : IIntegrationPointToSyncConverter, IIntegrationPointToSyncAppConverter
	{
		private readonly ISerializer _serializer;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobHistorySyncService _jobHistorySyncService;
		private readonly ISyncOperationsWrapper _syncOperations;
		private readonly IAPILog _logger;

		public IntegrationPointToSyncConverter(
			ISerializer serializer,
			IJobHistoryService jobHistoryService,
			IJobHistorySyncService jobHistorySyncService,
			ISyncOperationsWrapper syncOperations,
			IRelativityObjectManager relativityObjectManager,
			IAPILog logger)
		{
			_serializer = serializer;
			_jobHistoryService = jobHistoryService;
			_jobHistorySyncService = jobHistorySyncService;
			_syncOperations = syncOperations;
			_logger = logger;
		}

		public async Task<int> CreateSyncConfigurationAsync(int workspaceId, IntegrationPointDto integrationPointDto, int jobHistoryId, int userId)
		{
			try
			{
				IExtendedJob extendedJob = new ExtendedJobForSyncApplication
				{
					IntegrationPointId = integrationPointDto.ArtifactId,
					IntegrationPointDto = integrationPointDto,
					JobHistoryId = jobHistoryId,
					WorkspaceId = workspaceId,
					SubmittedById = userId
				};

				int syncConfigurationId = await CreateSyncConfigurationAsync(extendedJob).ConfigureAwait(false);
				return syncConfigurationId;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to create Sync configuration for Sync application");
				throw;
			}
		}

		public async Task<int> CreateSyncConfigurationAsync(IExtendedJob job)
		{
			SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(job.IntegrationPointDto.SourceConfiguration);
			ImportSettings importSettings = _serializer.Deserialize<ImportSettings>(job.IntegrationPointDto.DestinationConfiguration);
			FolderConf folderConf = _serializer.Deserialize<FolderConf>(job.IntegrationPointDto.DestinationConfiguration);

			ISyncContext syncContext = new SyncContext(
				job.WorkspaceId,
				sourceConfiguration.TargetWorkspaceArtifactId,
				job.JobHistoryId,
				Core.Constants.IntegrationPoints.APPLICATION_NAME,
				GetVersion());

			ISyncConfigurationBuilder builder = _syncOperations.GetSyncConfigurationBuilder(syncContext);

			if (importSettings.ArtifactTypeId != (int)ArtifactType.Document)
			{
				return await CreateNonDocumentSyncConfigurationAsync(builder, job, sourceConfiguration, importSettings).ConfigureAwait(false);
			}
			else
			{
				JobHistory jobHistory = _jobHistoryService.GetJobHistory(new List<int> { job.JobHistoryId }).FirstOrDefault();

				if (jobHistory != null)
				{
					importSettings.ImportOverwriteMode = NameToEnumConvert.GetEnumByModeName(jobHistory.Overwrite);
				}

				return importSettings.ImageImport ?
					await CreateImageSyncConfigurationAsync(builder, job, sourceConfiguration, importSettings).ConfigureAwait(false)
					: await CreateDocumentSyncConfigurationAsync(builder, job, sourceConfiguration, importSettings, folderConf).ConfigureAwait(false);
			}
		}

		private async Task<int> CreateImageSyncConfigurationAsync(
			ISyncConfigurationBuilder builder,
			IExtendedJob job,
			SourceConfiguration sourceConfiguration,
			ImportSettings importSettings)
		{
			IEnumerable<int> productionImagePrecedenceIds = importSettings.ProductionPrecedence == "1" ?
				importSettings.ImagePrecedence.Select(x => int.Parse(x.ArtifactID)) :
				Enumerable.Empty<int>();

			IImageSyncConfigurationBuilder syncConfigurationRoot = builder
				.ConfigureRdos(RdoConfiguration.GetRdoOptions())
				.ConfigureImageSync(
					new ImageSyncOptions(
						DataSourceType.SavedSearch,
						sourceConfiguration.SavedSearchArtifactId,
						DestinationLocationType.Folder,
						importSettings.DestinationFolderArtifactId)
					{
						CopyImagesMode = importSettings.ImportNativeFileCopyMode.ToSyncImageMode(),
						EnableTagging = importSettings.EnableTagging
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

				syncConfigurationRoot.IsRetry(new RetryOptions(jobToRetry.ArtifactID));
			}

			if (job.IntegrationPointDto.LogErrors == false)
			{
				syncConfigurationRoot.DisableItemLevelErrorLogging();
			}

			return await syncConfigurationRoot.SaveAsync().ConfigureAwait(false);
		}

		private async Task<int> CreateDocumentSyncConfigurationAsync(
			ISyncConfigurationBuilder builder,
			IExtendedJob job,
			SourceConfiguration sourceConfiguration,
			ImportSettings importSettings,
			FolderConf folderConf)
		{
			IDocumentSyncConfigurationBuilder syncConfigurationRoot = builder
				.ConfigureRdos(RdoConfiguration.GetRdoOptions())
				.ConfigureDocumentSync(
					new DocumentSyncOptions(
						sourceConfiguration.SavedSearchArtifactId,
						importSettings.DestinationFolderArtifactId)
					{
						CopyNativesMode = importSettings.ImportNativeFileCopyMode.ToSyncNativeMode(),
						EnableTagging = importSettings.EnableTagging
					})
				.WithFieldsMapping(mappingBuilder => PrepareFieldsMappingAction(
					job.IntegrationPointDto.FieldMappings, mappingBuilder))
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

				syncConfigurationRoot.IsRetry(new RetryOptions(jobToRetry.ArtifactID));
			}

			if (job.IntegrationPointDto.LogErrors == false)
			{
				syncConfigurationRoot.DisableItemLevelErrorLogging();
			}

			return await syncConfigurationRoot.SaveAsync().ConfigureAwait(false);
		}

		private async Task<int> CreateNonDocumentSyncConfigurationAsync(
			ISyncConfigurationBuilder builder,
			IExtendedJob job,
			SourceConfiguration sourceConfiguration,
			ImportSettings importSettings)
		{
			INonDocumentSyncConfigurationBuilder syncConfigurationRoot = builder
				.ConfigureRdos(RdoConfiguration.GetRdoOptions())
				.ConfigureNonDocumentSync(
					new NonDocumentSyncOptions(
						sourceConfiguration.SourceViewId,
						importSettings.ArtifactTypeId,
						importSettings.DestinationArtifactTypeId))
				.WithFieldsMapping(mappingBuilder => PrepareFieldsMappingAction(
					job.IntegrationPointDto.FieldMappings, mappingBuilder))
				.EmailNotifications(
					GetEmailOptions(job))
				.OverwriteMode(
					new OverwriteOptions(
						importSettings.ImportOverwriteMode.ToSyncImportOverwriteMode())
					{
						FieldsOverlayBehavior = importSettings.ImportOverlayBehavior.ToSyncFieldOverlayBehavior()
					});

			if (job.IntegrationPointDto.LogErrors == false)
			{
				syncConfigurationRoot.DisableItemLevelErrorLogging();
			}

			return await syncConfigurationRoot.SaveAsync().ConfigureAwait(false);
		}

		private void PrepareFieldsMappingAction(List<FieldMap> integrationPointsFieldsMapping, IFieldsMappingBuilder mappingBuilder)
		{
			List<SyncFieldMap> fieldsMapping = FieldMapHelper.FixedSyncMapping(integrationPointsFieldsMapping, _logger);

			SyncFieldMap identifier = fieldsMapping.FirstOrDefault(x => x.FieldMapType == FieldMapType.Identifier);
			if (identifier != null)
			{
				mappingBuilder.WithIdentifier();
			}

			foreach (SyncFieldMap fieldsMap in fieldsMapping.Where(x => x.FieldMapType == FieldMapType.None))
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
			if (job.IntegrationPointDto.EmailNotificationRecipients == null)
			{
				return new EmailNotificationsOptions(new List<string>());
			}

			List<string> emailsList = job.IntegrationPointDto
				.EmailNotificationRecipients
				.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim())
				.ToList();

			return new EmailNotificationsOptions(emailsList);
		}

		private bool IsRetryingErrors(Job job)
		{
			string jobDetails = job?.JobDetails;

			if (string.IsNullOrWhiteSpace(jobDetails))
			{
				return false;
			}

			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(jobDetails);
			JobHistory jobHistory = _jobHistoryService.GetRdo(taskParameters.BatchInstance);

			if (jobHistory == null)
			{
				// this means that job is scheduled, so it's not retrying errors
				return false;
			}

			return jobHistory.JobType.EqualsToChoice(JobTypeChoices.JobHistoryRetryErrors);
		}

		private Version GetVersion()
		{
			Version assemblyVersion;

			if (!Version.TryParse(typeof(IntegrationPointToSyncConverter).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version, out assemblyVersion))
			{
				_logger.LogWarning("Couldn't parse Version from AssemblyFileVersionAttribute");
			}

			return assemblyVersion;
		}
	}
}
