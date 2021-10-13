using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class IntegrationPointProfileHelper: WorkspaceHelperBase
    {
        private readonly ISerializer _serializer;

        public IntegrationPointProfileHelper(WorkspaceTest workspace, ISerializer serializer) : base(workspace)
        {
            _serializer = serializer;
        }

        public IntegrationPointProfileTest CreateEmptyIntegrationPointProfile()
        {
            var integrationPoint = new IntegrationPointProfileTest();

            Workspace.IntegrationPointProfiles.Add(integrationPoint);

            return integrationPoint;
        }

		public IntegrationPointProfileTest CreateSavedSearchIntegrationPoint(WorkspaceTest destinationWorkspace)
		{
			IntegrationPointProfileTest integrationPoint = CreateEmptyIntegrationPointProfile();

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
				ArtifactTypeId = (int)ArtifactType.Document,
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
	}
}
