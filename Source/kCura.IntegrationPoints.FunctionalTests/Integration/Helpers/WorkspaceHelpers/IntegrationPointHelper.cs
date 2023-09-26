using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Common;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using static Relativity.IntegrationPoints.Tests.Integration.Const;
using ImportType = Relativity.IntegrationPoints.Tests.Integration.Models.ImportType;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class IntegrationPointHelper : WorkspaceHelperBase
    {
        private readonly ISerializer _serializer;

        public IntegrationPointHelper(WorkspaceFake workspace, ISerializer serializer) : base(workspace)
        {
            _serializer = serializer;
        }

        public IntegrationPointFake CreateEmptyIntegrationPoint()
        {
            var integrationPoint = new IntegrationPointFake();

            Workspace.IntegrationPoints.Add(integrationPoint);

            return integrationPoint;
        }

        public IntegrationPointFake CreateSavedSearchSyncIntegrationPointWithErrors(WorkspaceFake destinationWorkspace)
        {
            IntegrationPointFake integrationPoint = CreateSavedSearchSyncIntegrationPoint(destinationWorkspace);
            integrationPoint.HasErrors = true;
            return integrationPoint;
        }

        public IntegrationPointFake CreateSavedSearchSyncIntegrationPoint(WorkspaceFake destinationWorkspace)
        {
            IntegrationPointFake integrationPoint = CreateEmptyIntegrationPoint();

            FolderFake destinationFolder = destinationWorkspace.Folders.First();

            SavedSearchFake sourceSavedSearch = Workspace.SavedSearches.First();

            IntegrationPointTypeFake integrationPointType = Workspace.IntegrationPointTypes.First(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString());

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

            integrationPoint.LogErrors = false;
            integrationPoint.EmailNotificationRecipients = string.Empty;
            integrationPoint.SourceProvider = sourceProvider.ArtifactId;
            integrationPoint.EnableScheduler = true;
            integrationPoint.ScheduleRule = ScheduleRuleTest.CreateWeeklyRule(
                    new DateTime(2021, 3, 20), new DateTime(2021, 3, 30), TimeZoneInfo.Utc, DaysOfWeek.Friday)
                .Serialize();
            integrationPoint.DestinationProvider = destinationProvider.ArtifactId;
            integrationPoint.Type = integrationPointType.ArtifactId;

            return integrationPoint;
        }

        public IntegrationPointFake CreateNonDocumentSyncIntegrationPoint(WorkspaceFake destinationWorkspace)
        {
            int artifactTypeId = GetArtifactTypeIdByName(Entity._ENTITY_OBJECT_NAME);

            IntegrationPointFake integrationPoint = CreateEmptyIntegrationPoint();

            ViewFake sourceView = Workspace.Views.First();

            IntegrationPointTypeFake integrationPointType = Workspace.IntegrationPointTypes.First(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString());

            SourceProviderFake sourceProvider = Workspace.SourceProviders.First(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);

            DestinationProviderFake destinationProvider = Workspace.DestinationProviders.First(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);

            List<FieldMap> fieldsMapping = Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierAndFirstAndLastNameFieldsMappingForEntitySync(destinationWorkspace);

            integrationPoint.FieldMappings = _serializer.Serialize(fieldsMapping);
            integrationPoint.SourceConfiguration = _serializer.Serialize(new SourceConfiguration
            {
                SourceWorkspaceArtifactId = Workspace.ArtifactId,
                TargetWorkspaceArtifactId = destinationWorkspace.ArtifactId,
                TypeOfExport = SourceConfiguration.ExportType.View,
                SourceViewId = sourceView.ArtifactId,
            });
            integrationPoint.DestinationConfiguration = CreateDestinationConfiguration(
                caseArtifactId: destinationWorkspace.ArtifactId,
                artifactTypeId: artifactTypeId);

            integrationPoint.SourceProvider = sourceProvider.ArtifactId;
            integrationPoint.EnableScheduler = true;
            integrationPoint.ScheduleRule = ScheduleRuleTest.CreateWeeklyRule(
                    new DateTime(2021, 3, 20), new DateTime(2021, 3, 30), TimeZoneInfo.Utc, DaysOfWeek.Friday)
                .Serialize();
            integrationPoint.DestinationProvider = destinationProvider.ArtifactId;
            integrationPoint.Type = integrationPointType.ArtifactId;

            return integrationPoint;
        }

        public IntegrationPointFake CreateImportIntegrationPointWithEntities(SourceProviderFake sourceProvider,
            string identifierFieldName, string sourceProviderConfiguration)
        {
            IntegrationPointFake integrationPoint = CreateImportIntegrationPoint(sourceProvider, identifierFieldName, sourceProviderConfiguration);
            integrationPoint.DestinationConfiguration = CreateDestinationConfiguration(
                caseArtifactId: Workspace.ArtifactId,
                artifactTypeId: GetArtifactTypeIdByName(Entity._ENTITY_OBJECT_NAME),
                entityManagerFieldContainsLink: true,
                destinationFolderArtifactId: Workspace.Folders.First().ArtifactId);

            List<FieldMap> fieldsMapping = Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierAndFirstAndLastNameFieldsMappingForEntitySync();
            integrationPoint.FieldMappings = _serializer.Serialize(fieldsMapping);

            return integrationPoint;
        }

        public IntegrationPointFake CreateImportIntegrationPoint(SourceProviderFake sourceProvider, string identifierFieldName, string sourceProviderConfiguration)
        {
            IntegrationPointFake integrationPoint = CreateEmptyIntegrationPoint();

            FolderFake destinationFolder = Workspace.Folders.First();

            IntegrationPointTypeFake integrationPointType = Workspace.IntegrationPointTypes.First(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes
                    .ImportGuid.ToString());

            DestinationProviderFake destinationProvider = Workspace.DestinationProviders.First(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders
                    .RELATIVITY);

            List<FieldMap> fieldsMapping =
                Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierFieldsMappingForImport(identifierFieldName);

            integrationPoint.FieldMappings = _serializer.Serialize(fieldsMapping);

            integrationPoint.SourceConfiguration = sourceProviderConfiguration;
            integrationPoint.DestinationConfiguration = CreateDestinationConfiguration(
                caseArtifactId: Workspace.ArtifactId,
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

        public IntegrationPointFake CreateImportDocumentLoadFileIntegrationPoint(string loadFile)
        {
            IntegrationPointFake integrationPoint = CreateEmptyIntegrationPoint();

            integrationPoint.Name = $"Import LoadFile - {Guid.NewGuid()}";
            List<FieldMap> fieldsMapping =
                Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierFieldsMappingForLoadFileImport("Control Number");

            integrationPoint.FieldMappings = _serializer.Serialize(fieldsMapping);

            SourceProviderFake sourceProvider = Workspace.SourceProviders.Single(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE);

            integrationPoint.SourceProvider = sourceProvider.ArtifactId;

            DestinationProviderFake destinationProvider = Workspace.DestinationProviders.Single(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);

            integrationPoint.DestinationProvider = destinationProvider.ArtifactId;

            integrationPoint.Type = Workspace.IntegrationPointTypes.Single(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes
                    .ImportGuid.ToString()).ArtifactId;

            FolderFake destinationFolder = Workspace.Folders.First();

            ImportProviderSettings sourceConfiguration = new ImportProviderSettings
            {
                EncodingType = "utf-8",
                AsciiColumn = 124,
                AsciiQuote = 94,
                AsciiNewLine = 174,
                AsciiMultiLine = 59,
                AsciiNestedValue = 92,
                WorkspaceId = Workspace.ArtifactId,
                ImportType = ((int)ImportType.DocumentLoadFile).ToString(),
                LoadFile = loadFile,
                LineNumber = "0",
                DestinationFolderArtifactId = destinationFolder.ArtifactId
            };

            integrationPoint.SourceConfiguration = _serializer.Serialize(sourceConfiguration);

            integrationPoint.DestinationConfiguration = CreateDestinationConfiguration(
                caseArtifactId: Workspace.ArtifactId,
                destinationFolderArtifactId: destinationFolder.ArtifactId);

            return integrationPoint;

        }

        public IntegrationPointFake CreateImportEntityFromLdapIntegrationPoint(bool linkEntityManagers = false, bool isMappingIdentifierOnly = false)
        {
            const string ou = "ou=Management";

            IntegrationPointFake integrationPoint = CreateEmptyIntegrationPoint();

            integrationPoint.Name = $"Import Entity LDAP - {Guid.NewGuid()}";

            List<FieldMap> fieldsMapping = isMappingIdentifierOnly
                ? Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierOnlyFieldsMappingForLDAPEntityImport()
                : Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierAndFirstAndLastNameFieldsMappingForLDAPEntityImport();
            integrationPoint.FieldMappings = _serializer.Serialize(fieldsMapping);

            SourceProviderFake sourceProvider = Workspace.SourceProviders.Single(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.LDAP);

            integrationPoint.SourceProvider = sourceProvider.ArtifactId;

            DestinationProviderFake destinationProvider = Workspace.DestinationProviders.Single(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);

            integrationPoint.DestinationProvider = destinationProvider.ArtifactId;

            integrationPoint.Type = Workspace.IntegrationPointTypes.Single(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes
                    .ImportGuid.ToString()).ArtifactId;

            LDAPSettings sourceConfiguration = new LDAPSettings
            {
                ConnectionPath = GlobalConst.LDAP._OPEN_LDAP_CONNECTION_PATH(ou),
                ConnectionAuthenticationType = AuthenticationTypesEnum.FastBind,
                ImportNested = false
            };

            integrationPoint.SourceConfiguration = _serializer.Serialize(sourceConfiguration);

            integrationPoint.DestinationConfiguration = CreateDestinationConfiguration(
                caseArtifactId: Workspace.ArtifactId,
                artifactTypeId: GetArtifactTypeIdByName(Entity._ENTITY_OBJECT_NAME),
                entityManagerFieldContainsLink: linkEntityManagers);

            LDAPSecuredConfiguration securedConfiguration = new LDAPSecuredConfiguration
            {
                UserName = GlobalConst.LDAP._OPEN_LDAP_USER,
                Password = GlobalConst.LDAP._OPEN_LDAP_PASSWORD
            };

            integrationPoint.SecuredConfiguration = Guid.NewGuid().ToString();

            integrationPoint.SecuredConfigurationDecrypted = _serializer.Serialize(securedConfiguration);

            return integrationPoint;
        }

        public IntegrationPointDto CreateSavedSearchIntegrationPointModel(WorkspaceFake destinationWorkspace)
        {
            IntegrationPointFake integrationPoint = CreateSavedSearchSyncIntegrationPoint(destinationWorkspace);
            return integrationPoint.ToDto();
        }

        public void RemoveIntegrationPoint(int integrationPointId)
        {
            foreach (IntegrationPointFake integrationPoint in Workspace.IntegrationPoints.Where(x => x.ArtifactId == integrationPointId).ToArray())
            {
                Workspace.IntegrationPoints.Remove(integrationPoint);
            }
        }

        private int GetArtifactTypeIdByName(string name)
        {
            return Workspace.ObjectTypes.First(x => x.Name == name).ArtifactTypeId;
        }

        private string CreateDestinationConfiguration(
            int caseArtifactId,
            int destinationFolderArtifactId = 0,
            int artifactTypeId = (int)ArtifactType.Document,
            bool entityManagerFieldContainsLink = false,
            string overlayIdentifier = EntityFieldNames.UniqueId)
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
                OverlayIdentifier = overlayIdentifier,
            });
        }
    }
}
