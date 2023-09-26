using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class IntegrationPointProfileHelper : WorkspaceHelperBase
    {
        private readonly ISerializer _serializer;

        public IntegrationPointProfileHelper(WorkspaceFake workspace, ISerializer serializer) : base(workspace)
        {
            _serializer = serializer;
        }

        public IntegrationPointProfileFake CreateEmptyIntegrationPointProfile()
        {
            var integrationPoint = new IntegrationPointProfileFake();

            Workspace.IntegrationPointProfiles.Add(integrationPoint);

            return integrationPoint;
        }

        public IntegrationPointProfileFake CreateSavedSearchIntegrationPointProfile(WorkspaceFake destinationWorkspace)
        {
            IntegrationPointProfileFake integrationPoint = CreateEmptyIntegrationPointProfile();

            FolderFake destinationFolder = destinationWorkspace.Folders.First();

            SavedSearchFake sourceSavedSearch = Workspace.SavedSearches.First();

            IntegrationPointTypeFake integrationPointType = Workspace.IntegrationPointTypes.First(x => x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString());

            SourceProviderFake sourceProvider = Workspace.SourceProviders.First(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);

            DestinationProviderFake destinationProvider = Workspace.DestinationProviders.First(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);

            List<FieldMap> fieldsMapping = Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierFieldsMapping(destinationWorkspace, (int)ArtifactType.Document);

            integrationPoint.FieldMappings = _serializer.Serialize(fieldsMapping);
            integrationPoint.SourceConfiguration = _serializer.Serialize(new SourceConfiguration
            {
                SourceWorkspaceArtifactId = Workspace.ArtifactId,
                TargetWorkspaceArtifactId = destinationWorkspace.ArtifactId,
                TypeOfExport = SourceConfiguration.ExportType.SavedSearch,
                SavedSearchArtifactId = sourceSavedSearch.ArtifactId,
            });

            integrationPoint.DestinationConfiguration = CreateDestinationConfiguration(
                caseArtifactId: destinationWorkspace.ArtifactId,
                destinationFolderArtifactId: destinationFolder.ArtifactId);

            integrationPoint.SourceProvider = sourceProvider.ArtifactId;
            integrationPoint.EnableScheduler = true;
            integrationPoint.ScheduleRule = ScheduleRuleTest.CreateWeeklyRule(
                    new DateTime(2021, 3, 20), new DateTime(2021, 3, 30), TimeZoneInfo.Utc, DaysOfWeek.Friday)
                .Serialize();
            integrationPoint.DestinationProvider = destinationProvider.ArtifactId;
            integrationPoint.Type = integrationPointType.ArtifactId;

            return integrationPoint;
        }

        public IntegrationPointProfileFake CreateSavedSearchIntegrationPointProfileWithDeserializableSourceConfiguration(WorkspaceFake destinationWorkspace, int longTextLimit)
        {
            IntegrationPointProfileFake integrationPointProfile = CreateSavedSearchIntegrationPointProfile(destinationWorkspace);
            SavedSearchFake sourceSavedSearch = Workspace.SavedSearches.First();
            integrationPointProfile.SourceConfiguration = _serializer.Serialize(new
            {
                SourceWorkspaceArtifactId = Workspace.ArtifactId,
                TargetWorkspaceArtifactId = destinationWorkspace.ArtifactId,
                TypeOfExport = SourceConfiguration.ExportType.SavedSearch,
                SavedSearchArtifactId = sourceSavedSearch.ArtifactId,
                Filler = new String(Enumerable.Repeat('-', longTextLimit).ToArray())
            });

            return integrationPointProfile;
        }

        public IntegrationPointProfileFake CreateSavedSearchIntegrationPointProfileWithDeserializableFieldMappings(WorkspaceFake destinationWorkspace, int longTextLimit)
        {
            IntegrationPointProfileFake integrationPointProfile = CreateSavedSearchIntegrationPointProfile(destinationWorkspace);
            List<FieldMap> fieldsMapping = Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierFieldsMapping(destinationWorkspace, (int)ArtifactType.Document);
            fieldsMapping[0].SourceField.DisplayName = new string(Enumerable.Repeat('-', longTextLimit / 2).ToArray());
            fieldsMapping[0].DestinationField.DisplayName = new string(Enumerable.Repeat('-', longTextLimit / 2).ToArray());
            integrationPointProfile.FieldMappings = _serializer.Serialize(fieldsMapping);

            return integrationPointProfile;
        }

        public IntegrationPointProfileFake CreateSavedSearchIntegrationPointProfileWithDeserializableDestinationConfiguration(WorkspaceFake destinationWorkspace, int longTextLimit)
        {
            IntegrationPointProfileFake integrationPointProfile = CreateSavedSearchIntegrationPointProfile(destinationWorkspace);
            FolderFake destinationFolder = destinationWorkspace.Folders.First();
            integrationPointProfile.DestinationConfiguration = CreateDestinationConfiguration(
                caseArtifactId: destinationWorkspace.ArtifactId,
                destinationFolderArtifactId: destinationFolder.ArtifactId);

            return integrationPointProfile;
        }

        public IntegrationPointProfileDto CreateSavedSearchIntegrationPointAsIntegrationPointProfileModel(WorkspaceFake destinationWorkspace)
        {
            IntegrationPointProfileFake integrationPointProfile = CreateSavedSearchIntegrationPointProfile(destinationWorkspace);
            IntegrationPointProfileDto integrationPointProfileDto = new IntegrationPointProfileDto
            {
                Name = integrationPointProfile.Name,
                SelectedOverwrite = integrationPointProfile.OverwriteFields == null ? string.Empty : integrationPointProfile.OverwriteFields.Name,
                SourceProvider = integrationPointProfile.SourceProvider.GetValueOrDefault(0),
                DestinationConfiguration = _serializer.Deserialize<DestinationConfiguration>(integrationPointProfile.DestinationConfiguration),
                SourceConfiguration = integrationPointProfile.SourceConfiguration,
                DestinationProvider = integrationPointProfile.DestinationProvider.GetValueOrDefault(0),
                Type = integrationPointProfile.Type,
                Scheduler = new Scheduler(integrationPointProfile.EnableScheduler.GetValueOrDefault(false), integrationPointProfile.ScheduleRule),
                EmailNotificationRecipients = integrationPointProfile.EmailNotificationRecipients ?? string.Empty,
                LogErrors = integrationPointProfile.LogErrors.GetValueOrDefault(false),
                NextRun = integrationPointProfile.NextScheduledRuntimeUTC,
                FieldMappings = _serializer.Deserialize<List<FieldMap>>(integrationPointProfile.FieldMappings),
            };

            Workspace.IntegrationPointProfiles.Remove(integrationPointProfile);
            return integrationPointProfileDto;
        }

        private string CreateDestinationConfiguration(
            int caseArtifactId,
            int destinationFolderArtifactId = 0,
            int artifactTypeId = (int)ArtifactType.Document,
            bool entityManagerFieldContainsLink = false)
        {
            return _serializer.Serialize(new DestinationConfiguration()
            {
                ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
                FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,

                CaseArtifactId = caseArtifactId,
                ArtifactTypeId = artifactTypeId,
                DestinationArtifactTypeId = artifactTypeId,
                DestinationFolderArtifactId = destinationFolderArtifactId,
                EntityManagerFieldContainsLink = entityManagerFieldContainsLink,
            });
        }
    }
}
