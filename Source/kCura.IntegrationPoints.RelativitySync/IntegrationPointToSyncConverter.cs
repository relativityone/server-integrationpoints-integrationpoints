using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.RelativitySync;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Utils;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.RelativitySync.Utils;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.SyncConfiguration;
using Relativity.Sync.SyncConfiguration.FieldsMapping;
using Relativity.Sync.SyncConfiguration.Options;
using Enumerable = System.Linq.Enumerable;
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
        private readonly IRipToggleProvider _toggleProvider;
        private readonly IAPILog _logger;

        public IntegrationPointToSyncConverter(
            ISerializer serializer,
            IJobHistoryService jobHistoryService,
            IJobHistorySyncService jobHistorySyncService,
            ISyncOperationsWrapper syncOperations,
            IRipToggleProvider toggleProvider,
            IAPILog logger)
        {
            _serializer = serializer;
            _jobHistoryService = jobHistoryService;
            _jobHistorySyncService = jobHistorySyncService;
            _syncOperations = syncOperations;
            _logger = logger;
            _toggleProvider = toggleProvider;
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
            DestinationConfiguration destinationConfiguration = job.IntegrationPointDto.DestinationConfiguration;

            _logger
                .ForContext("DestinationConfiguration", destinationConfiguration, true)
                .ForContext("SourceConfiguration", sourceConfiguration, true)
                .LogInformation("Read Integration Point Configuration {integrationPointId}", job.IntegrationPointId);

            ISyncContext syncContext = new SyncContext(job.WorkspaceId, sourceConfiguration.TargetWorkspaceArtifactId, job.JobHistoryId,
                Core.Constants.IntegrationPoints.APPLICATION_NAME, GetVersion());

            ISyncConfigurationBuilder builder = _syncOperations.GetSyncConfigurationBuilder(syncContext);

            if (!_toggleProvider.IsEnabledByName("kCura.IntegrationPoints.Common.Toggles.EnableTaggingToggle"))
            {
                destinationConfiguration.EnableTagging = true;
            }

            if (destinationConfiguration.ArtifactTypeId != (int)ArtifactType.Document)
            {
                return await CreateNonDocumentSyncConfigurationAsync(builder, job, sourceConfiguration, destinationConfiguration).ConfigureAwait(false);
            }
            else
            {
                JobHistory jobHistory = _jobHistoryService.GetRdoWithoutDocuments(job.JobHistoryId);

                if (jobHistory != null)
                {
                    destinationConfiguration.ImportOverwriteMode = NameToEnumConvert.GetEnumByModeName(jobHistory.Overwrite);
                }

                return destinationConfiguration.ImageImport ?
                    await CreateImageSyncConfigurationAsync(builder, job, jobHistory, sourceConfiguration, destinationConfiguration).ConfigureAwait(false)
                    : await CreateDocumentSyncConfigurationAsync(builder, job, jobHistory, sourceConfiguration, destinationConfiguration).ConfigureAwait(false);
            }
        }

        private async Task<int> CreateImageSyncConfigurationAsync(
            ISyncConfigurationBuilder builder,
            IExtendedJob job,
            JobHistory jobHistory,
            SourceConfiguration sourceConfiguration,
            DestinationConfiguration destinationConfiguration)
        {
            IEnumerable<int> productionImagePrecedenceIds = destinationConfiguration.ProductionPrecedence == (int)ExportSettings.ProductionPrecedenceType.Produced
                ? destinationConfiguration.ImagePrecedence.Select(x => int.Parse(x.ArtifactID))
                : Enumerable.Empty<int>();

            IImageSyncConfigurationBuilder syncConfigurationRoot = builder
                .ConfigureRdos(RdoConfiguration.GetRdoOptions())
                .ConfigureImageSync(
                    new ImageSyncOptions(
                        DataSourceType.SavedSearch, sourceConfiguration.SavedSearchArtifactId,
                        DestinationLocationType.Folder, destinationConfiguration.DestinationFolderArtifactId)
                    {
                        CopyImagesMode = destinationConfiguration.ImportNativeFileCopyMode.ToSyncImageMode(),
                        TaggingOption = destinationConfiguration.TaggingOption.ToSyncTaggingOption()
                    })
                .ProductionImagePrecedence(
                    new ProductionImagePrecedenceOptions(
                        productionImagePrecedenceIds,
                        destinationConfiguration.IncludeOriginalImages))
                .EmailNotifications(
                    GetEmailOptions(job))
                .OverwriteMode(
                    new OverwriteOptions(
                        destinationConfiguration.ImportOverwriteMode.ToSyncImportOverwriteMode())
                    {
                        FieldsOverlayBehavior = destinationConfiguration.FieldOverlayBehavior.ToSyncFieldOverlayBehavior()
                    })
                .CreateSavedSearch(
                    new CreateSavedSearchOptions(
                        destinationConfiguration.CreateSavedSearchForTagging));

            if (IsRetryingErrors(jobHistory))
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
            JobHistory jobHistory,
            SourceConfiguration sourceConfiguration,
            DestinationConfiguration destinationConfiguration)
        {
            DateTime? smartOverwriteDate = await GetSmartOverwriteDateAsync(destinationConfiguration, job.WorkspaceId, job.IntegrationPointId).ConfigureAwait(false);

            IDocumentSyncConfigurationBuilder syncConfigurationRoot = builder
                .ConfigureRdos(RdoConfiguration.GetRdoOptions())
                .ConfigureDocumentSync(
                    new DocumentSyncOptions(
                        sourceConfiguration.SavedSearchArtifactId,
                        destinationConfiguration.DestinationFolderArtifactId)
                    {
                        CopyNativesMode = destinationConfiguration.ImportNativeFileCopyMode.ToSyncNativeMode(),
                        TaggingOption = destinationConfiguration.TaggingOption.ToSyncTaggingOption()
                    })
                .WithFieldsMapping(mappingBuilder => PrepareFieldsMappingAction(
                    job.IntegrationPointDto.FieldMappings, mappingBuilder))
                .DestinationFolderStructure(
                    GetFolderStructureOptions(destinationConfiguration))
                .EmailNotifications(
                    GetEmailOptions(job))
                .OverwriteMode(
                    new OverwriteOptions(
                        destinationConfiguration.ImportOverwriteMode.ToSyncImportOverwriteMode())
                    {
                        FieldsOverlayBehavior = destinationConfiguration.FieldOverlayBehavior.ToSyncFieldOverlayBehavior(),
                        SmartOverwriteDate = smartOverwriteDate
                    })
                .CreateSavedSearch(
                    new CreateSavedSearchOptions(
                        destinationConfiguration.CreateSavedSearchForTagging));
            if (IsRetryingErrors(jobHistory))
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

        private async Task<DateTime?> GetSmartOverwriteDateAsync(DestinationConfiguration destinationConfiguration, int workspaceId, int integrationPointId)
        {
            if (!await _toggleProvider.IsEnabledAsync<EnableSmartOverwriteFeatureToggle>().ConfigureAwait(false))
            {
                return null;
            }

            if (destinationConfiguration.TaggingOption == TaggingOptionEnum.Enabled || !destinationConfiguration.UseSmartOverwrite)
            {
                return null;
            }

            DateTime? date = await _jobHistorySyncService.GetLastCompletedJobHistoryForRunDateAsync(workspaceId, integrationPointId).ConfigureAwait(false);
            return date;
        }

        private async Task<int> CreateNonDocumentSyncConfigurationAsync(
            ISyncConfigurationBuilder builder,
            IExtendedJob job,
            SourceConfiguration sourceConfiguration,
            DestinationConfiguration destinationConfiguration)
        {
            INonDocumentSyncConfigurationBuilder syncConfigurationRoot = builder
                .ConfigureRdos(RdoConfiguration.GetRdoOptions())
                .ConfigureNonDocumentSync(
                    new NonDocumentSyncOptions(
                        sourceConfiguration.SourceViewId,
                        destinationConfiguration.ArtifactTypeId,
                        destinationConfiguration.GetDestinationArtifactTypeId()))
                .WithFieldsMapping(mappingBuilder => PrepareFieldsMappingAction(
                    job.IntegrationPointDto.FieldMappings, mappingBuilder))
                .EmailNotifications(
                    GetEmailOptions(job))
                .OverwriteMode(
                    new OverwriteOptions(
                        destinationConfiguration.ImportOverwriteMode.ToSyncImportOverwriteMode())
                    {
                        FieldsOverlayBehavior = destinationConfiguration.FieldOverlayBehavior.ToSyncFieldOverlayBehavior()
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

            foreach (SyncFieldMap fieldsMap in fieldsMapping.Where(x => x.FieldMapType == FieldMapType.None))
            {
                mappingBuilder.WithField(fieldsMap.SourceField.FieldIdentifier, fieldsMap.DestinationField.FieldIdentifier);
            }
        }

        private DestinationFolderStructureOptions GetFolderStructureOptions(DestinationConfiguration destinationConfiguration)
        {
            if (destinationConfiguration.UseFolderPathInformation)
            {
                DestinationFolderStructureOptions folderOptions = DestinationFolderStructureOptions.ReadFromField(destinationConfiguration.FolderPathSourceField);
                folderOptions.MoveExistingDocuments = destinationConfiguration.MoveExistingDocuments;

                return folderOptions;
            }

            if (destinationConfiguration.UseDynamicFolderPath)
            {
                DestinationFolderStructureOptions folderOptions = DestinationFolderStructureOptions.RetainFolderStructureFromSourceWorkspace();
                folderOptions.MoveExistingDocuments = destinationConfiguration.MoveExistingDocuments;

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

        private bool IsRetryingErrors(JobHistory jobHistory)
        {
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
