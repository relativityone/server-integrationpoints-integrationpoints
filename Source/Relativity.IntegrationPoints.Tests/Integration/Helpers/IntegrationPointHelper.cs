using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class IntegrationPointHelper : HelperBase
	{
		private readonly ISerializer _serializer;

		public IntegrationPointHelper(HelperManager manager, InMemoryDatabase database, ProxyMock proxyMock, ISerializer serializer) 
			: base(manager, database, proxyMock)
		{
			_serializer = serializer;
		}

		public IntegrationPointTest CreateEmptyIntegrationPoint(WorkspaceTest workspace)
		{
			var integrationPoint = new IntegrationPointTest
			{
				WorkspaceId = workspace.ArtifactId
			};

			Database.IntegrationPoints.Add(integrationPoint);

			return integrationPoint;
		}

		public IntegrationPointTest CreateSavedSearchIntegrationPoint(WorkspaceTest sourceWorkspace, WorkspaceTest destinationWorkspace)
		{
			IntegrationPointTest integrationPoint = CreateEmptyIntegrationPoint(sourceWorkspace);
			
			FolderTest destinationFolder = Database.Folders.First(x => x.WorkspaceId == destinationWorkspace.ArtifactId);

			SavedSearchTest sourceSavedSearch = Database.SavedSearches.First(x => x.WorkspaceId == sourceWorkspace.ArtifactId);

			IntegrationPointTypeTest integrationPointType = Database.IntegrationPointTypes.First(x => 
				x.WorkspaceId == sourceWorkspace.ArtifactId &&
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString());

			SourceProviderTest sourceProvider = Database.SourceProviders.First(x =>
				x.WorkspaceId == sourceWorkspace.ArtifactId &&
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);

			DestinationProviderTest destinationProvider = Database.DestinationProviders.First(x =>
				x.WorkspaceId == sourceWorkspace.ArtifactId &&
				x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);

			List<FieldMap> fieldsMapping = HelperManager.FieldsMappingHelper.PrepareIdentifierFieldsMapping(sourceWorkspace, destinationWorkspace);

			integrationPoint.WorkspaceId = sourceWorkspace.ArtifactId;
			integrationPoint.FieldMappings = _serializer.Serialize(fieldsMapping);
			integrationPoint.SourceConfiguration = _serializer.Serialize(new SourceConfiguration
			{
				SourceWorkspaceArtifactId = sourceWorkspace.ArtifactId,
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
				CaseArtifactId = destinationWorkspace.ArtifactId
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

		public void RemoveIntegrationPoint(int integrationPointId)
		{
			foreach (IntegrationPointTest integrationPoint in Database.IntegrationPoints.Where(x => x.ArtifactId == integrationPointId).ToArray())
			{
				Database.IntegrationPoints.Remove(integrationPoint);
			}
		}
	}
}
