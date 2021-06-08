using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;

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

		public IntegrationPointTest CreateSavedSearchIntegrationPoint(WorkspaceTest destinationWorkspace)
		{
			IntegrationPointTest integrationPoint = CreateEmptyIntegrationPoint();

			FolderTest destinationFolder = destinationWorkspace.Folders.First();

			SavedSearchTest sourceSavedSearch = Workspace.SavedSearches.First();

			IntegrationPointTypeTest integrationPointType = Workspace.IntegrationPointTypes.First(x => x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString());

			SourceProviderTest sourceProvider = Workspace.SourceProviders.First(x =>
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);

			DestinationProviderTest destinationProvider = Workspace.DestinationProviders.First(x =>
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);

			List<FieldMap> fieldsMapping = Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierFieldsMapping(destinationWorkspace);

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
				Workspace.Helpers.FieldsMappingHelper.PrepareIdentifierFieldsMappingForImport("Control Number");

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
			};

			integrationPoint.DestinationConfiguration = _serializer.Serialize(destinationConfiguration);

			return integrationPoint;

		}

		public void RemoveIntegrationPoint(int integrationPointId)
		{
			foreach (IntegrationPointTest integrationPoint in Workspace.IntegrationPoints.Where(x => x.ArtifactId == integrationPointId).ToArray())
			{
				Workspace.IntegrationPoints.Remove(integrationPoint);
			}
		}
	}
}
