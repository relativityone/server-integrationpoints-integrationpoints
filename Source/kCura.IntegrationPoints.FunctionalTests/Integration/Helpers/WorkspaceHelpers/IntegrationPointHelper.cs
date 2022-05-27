using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.IntegrationPoints.Tests.Common;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using static Relativity.IntegrationPoints.Tests.Integration.Const;
using FieldMap = Relativity.IntegrationPoints.FieldsMapping.Models.FieldMap;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
	public class IntegrationPointHelper : WorkspaceHelperBase
	{
		private readonly ISerializer _serializer;

		public IntegrationPointHelper(WorkspaceTest workspace, ISerializer serializer) : base(workspace)
		{
			_serializer = serializer;
		}

		public IntegrationPointTest CreateEmptyIntegrationPoint()
		{
			var integrationPoint = new IntegrationPointTest();

			Workspace.IntegrationPoints.Add(integrationPoint);

			return integrationPoint;
		}

        public IntegrationPointTest CreateSavedSearchSyncIntegrationPointWithErrors(WorkspaceTest destinationWorkspace)
        {
            IntegrationPointTest integrationPoint = CreateSavedSearchSyncIntegrationPoint(destinationWorkspace);
            integrationPoint.HasErrors = true;
            return integrationPoint;
        }

        public IntegrationPointTest CreateSavedSearchSyncIntegrationPoint(WorkspaceTest destinationWorkspace)
		{
			IntegrationPointTest integrationPoint = CreateEmptyIntegrationPoint();

			FolderTest destinationFolder = destinationWorkspace.Folders.First();

			SavedSearchTest sourceSavedSearch = Workspace.SavedSearches.First();

			IntegrationPointTypeTest integrationPointType = Workspace.IntegrationPointTypes.First(x => 
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString());

			SourceProviderTest sourceProvider = Workspace.SourceProviders.First(x =>
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);

			DestinationProviderTest destinationProvider = Workspace.DestinationProviders.First(x =>
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
			integrationPoint.DestinationConfiguration = _serializer.Serialize(new ImportSettings
			{
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
				ArtifactTypeId = (int) ArtifactType.Document,
				DestinationArtifactTypeId = (int) ArtifactType.Document,
				DestinationFolderArtifactId = destinationFolder.ArtifactId,
				CaseArtifactId = destinationWorkspace.ArtifactId,
				WebServiceURL = @"//some/service/url/relativity"
			});
			integrationPoint.SourceProvider = sourceProvider.ArtifactId;
			integrationPoint.EnableScheduler = true;
			integrationPoint.ScheduleRule = ScheduleRuleTest.CreateWeeklyRule(
					new DateTime(2021, 3, 20), new DateTime(2021, 3, 30), TimeZoneInfo.Utc, DaysOfWeek.Friday)
				.Serialize();
			integrationPoint.DestinationProvider = destinationProvider.ArtifactId;
			integrationPoint.Type = integrationPointType.ArtifactId;

			return integrationPoint;
		}

		public IntegrationPointTest CreateNonDocumentSyncIntegrationPoint(WorkspaceTest destinationWorkspace)
		{
			int artifactTypeId = GetArtifactTypeIdByName(Entity._ENTITY_OBJECT_NAME);

			IntegrationPointTest integrationPoint = CreateEmptyIntegrationPoint();
			
			ViewTest sourceView = Workspace.Views.First();

			IntegrationPointTypeTest integrationPointType = Workspace.IntegrationPointTypes.First(x => 
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString());

			SourceProviderTest sourceProvider = Workspace.SourceProviders.First(x =>
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);

			DestinationProviderTest destinationProvider = Workspace.DestinationProviders.First(x =>
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
            integrationPoint.DestinationConfiguration = _serializer.Serialize(new ImportSettings
            {
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
				ArtifactTypeId = artifactTypeId,
				DestinationArtifactTypeId = artifactTypeId,
				CaseArtifactId = destinationWorkspace.ArtifactId,
				WebServiceURL = @"//some/service/url/relativity"
			});
			integrationPoint.SourceProvider = sourceProvider.ArtifactId;
			integrationPoint.EnableScheduler = true;
			integrationPoint.ScheduleRule = ScheduleRuleTest.CreateWeeklyRule(
					new DateTime(2021, 3, 20), new DateTime(2021, 3, 30), TimeZoneInfo.Utc, DaysOfWeek.Friday)
				.Serialize();
			integrationPoint.DestinationProvider = destinationProvider.ArtifactId;
			integrationPoint.Type = integrationPointType.ArtifactId;

			return integrationPoint;
		}

        public IntegrationPointTest CreateImportIntegrationPointWithEntities(SourceProviderTest sourceProvider,
            string identifierFieldName, string sourceProviderConfiguration)
        {
            IntegrationPointTest integrationPoint =
                CreateImportIntegrationPoint(sourceProvider, identifierFieldName, sourceProviderConfiguration);

            ImportSettings destinationImportSettings = _serializer.Deserialize<ImportSettings>(integrationPoint.DestinationConfiguration);

            destinationImportSettings.ArtifactTypeId = GetArtifactTypeIdByName(Const.Entity._ENTITY_OBJECT_NAME);
            destinationImportSettings.EntityManagerFieldContainsLink = true;
            integrationPoint.DestinationConfiguration = _serializer.Serialize(destinationImportSettings);

            List<FieldMap> fieldsMapping = Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierAndFirstAndLastNameFieldsMappingForEntitySync();
            integrationPoint.FieldMappings = _serializer.Serialize(fieldsMapping);
			
			return integrationPoint;
        }

		public IntegrationPointTest CreateImportIntegrationPoint(SourceProviderTest sourceProvider, string identifierFieldName, string sourceProviderConfiguration)
		{
			IntegrationPointTest integrationPoint = CreateEmptyIntegrationPoint();

			FolderTest destinationFolder = Workspace.Folders.First();

			IntegrationPointTypeTest integrationPointType = Workspace.IntegrationPointTypes.First(x =>
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes
					.ImportGuid.ToString());

			DestinationProviderTest destinationProvider = Workspace.DestinationProviders.First(x =>
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders
					.RELATIVITY);

			List<FieldMap> fieldsMapping =
				Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierFieldsMappingForImport(identifierFieldName);

			integrationPoint.FieldMappings = _serializer.Serialize(fieldsMapping);

			integrationPoint.SourceConfiguration = sourceProviderConfiguration;
			integrationPoint.DestinationConfiguration = _serializer.Serialize(new ImportSettings
			{
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
				ArtifactTypeId = (int) ArtifactType.Document,
				DestinationFolderArtifactId = destinationFolder.ArtifactId,
				CaseArtifactId = Workspace.ArtifactId,
				WebServiceURL = @"//some/service/url/relativity"
			});

			integrationPoint.SourceProvider = sourceProvider.ArtifactId;
			integrationPoint.EnableScheduler = true;
			integrationPoint.ScheduleRule = ScheduleRuleTest.CreateWeeklyRule(
					new DateTime(2021, 3, 20), new DateTime(2021, 3, 30), TimeZoneInfo.Utc, DaysOfWeek.Friday)
				.Serialize();
			integrationPoint.DestinationProvider = destinationProvider.ArtifactId;
			integrationPoint.Type = integrationPointType.ArtifactId;

			return integrationPoint;
		}

        public IntegrationPointTest CreateImportDocumentLoadFileIntegrationPoint(string loadFile)
		{
			IntegrationPointTest integrationPoint = CreateEmptyIntegrationPoint();

			integrationPoint.Name = $"Import LoadFile - {Guid.NewGuid()}";
			List<FieldMap> fieldsMapping =
				Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierFieldsMappingForLoadFileImport("Control Number");

			integrationPoint.FieldMappings = _serializer.Serialize(fieldsMapping);

			SourceProviderTest sourceProvider = Workspace.SourceProviders.Single(x =>
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE);

			integrationPoint.SourceProvider = sourceProvider.ArtifactId;

			DestinationProviderTest destinationProvider = Workspace.DestinationProviders.Single(x =>
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);

			integrationPoint.DestinationProvider = destinationProvider.ArtifactId;

			integrationPoint.Type = Workspace.IntegrationPointTypes.Single(x =>
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes
					.ImportGuid.ToString()).ArtifactId;

			FolderTest destinationFolder = Workspace.Folders.First();

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

			ImportSettings destinationConfiguration = new ImportSettings
			{
				ArtifactTypeId = (int)ArtifactType.Document,
				DestinationProviderType = destinationProvider.Identifier,
				CaseArtifactId = Workspace.ArtifactId,
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				ImportNativeFile = false,
				ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles,
				UseDynamicFolderPath = false,
				DestinationFolderArtifactId = destinationFolder.ArtifactId,
				FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
				WebServiceURL = "https://fake.uri"
			};

			integrationPoint.DestinationConfiguration = _serializer.Serialize(destinationConfiguration);

			return integrationPoint;

		}

		public IntegrationPointTest CreateImportEntityFromLdapIntegrationPoint(bool linkEntityManagers = false, bool isMappingIdentifierOnly = false)
		{
			const string ou = "ou=Management";

			IntegrationPointTest integrationPoint = CreateEmptyIntegrationPoint();

			integrationPoint.Name = $"Import Entity LDAP - {Guid.NewGuid()}";

			List<FieldMap> fieldsMapping = isMappingIdentifierOnly 
				? Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierOnlyFieldsMappingForLDAPEntityImport() 
				: Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierAndFirstAndLastNameFieldsMappingForLDAPEntityImport();
			integrationPoint.FieldMappings = _serializer.Serialize(fieldsMapping);


			SourceProviderTest sourceProvider = Workspace.SourceProviders.Single(x =>
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.LDAP);

			integrationPoint.SourceProvider = sourceProvider.ArtifactId;

			DestinationProviderTest destinationProvider = Workspace.DestinationProviders.Single(x =>
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

			ImportSettings destinationConfiguration = new ImportSettings
			{
				ArtifactTypeId = GetArtifactTypeIdByName(Const.Entity._ENTITY_OBJECT_NAME),
				DestinationProviderType = destinationProvider.Identifier,
				EntityManagerFieldContainsLink = linkEntityManagers,
				CaseArtifactId = Workspace.ArtifactId,
				FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
				WebServiceURL = @"//some/service/url/relativity"
			};

			integrationPoint.DestinationConfiguration = _serializer.Serialize(destinationConfiguration);

			LDAPSecuredConfiguration securedConfiguration = new LDAPSecuredConfiguration
			{
				UserName = GlobalConst.LDAP._OPEN_LDAP_USER,
				Password = GlobalConst.LDAP._OPEN_LDAP_PASSWORD
			};

			integrationPoint.SecuredConfiguration = Guid.NewGuid().ToString();

			integrationPoint.SecuredConfigurationDecrypted = _serializer.Serialize(securedConfiguration);

			return integrationPoint;
		}

		public kCura.IntegrationPoints.Core.Models.IntegrationPointModel CreateSavedSearchIntegrationPointModel(WorkspaceTest destinationWorkspace)
        {
			IntegrationPointTest integrationPoint = CreateSavedSearchSyncIntegrationPoint(destinationWorkspace);
			return new kCura.IntegrationPoints.Core.Models.IntegrationPointModel
            {
				Name = integrationPoint.Name,
				ArtifactID = integrationPoint.ArtifactId,
				SelectedOverwrite = integrationPoint.OverwriteFields == null ? string.Empty : integrationPoint.OverwriteFields.Name,
				SourceProvider = integrationPoint.SourceProvider.GetValueOrDefault(0),
				Destination = integrationPoint.DestinationConfiguration,
				SourceConfiguration = integrationPoint.SourceConfiguration,
				DestinationProvider = integrationPoint.DestinationProvider.GetValueOrDefault(0),
				Type = integrationPoint.Type.GetValueOrDefault(0),
				Scheduler = new kCura.IntegrationPoints.Core.Models.Scheduler(integrationPoint.EnableScheduler.GetValueOrDefault(false), integrationPoint.ScheduleRule),
				NotificationEmails = integrationPoint.EmailNotificationRecipients ?? string.Empty,
				LogErrors = integrationPoint.LogErrors.GetValueOrDefault(false),
				NextRun = integrationPoint.NextScheduledRuntimeUTC,
				Map = integrationPoint.FieldMappings
			};
		}

		public void RemoveIntegrationPoint(int integrationPointId)
		{
			foreach (IntegrationPointTest integrationPoint in Workspace.IntegrationPoints.Where(x => x.ArtifactId == integrationPointId).ToArray())
			{
				Workspace.IntegrationPoints.Remove(integrationPoint);
			}
		}
		
		private int GetArtifactTypeIdByName(string name)
		{
			return Workspace.ObjectTypes.First(x => x.Name == name).ArtifactTypeId;
		}
	}
}
