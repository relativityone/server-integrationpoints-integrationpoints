using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.Keywords;
using kCura.IntegrationPoints.Services;
using Newtonsoft.Json;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class IntegrationPoint
	{
		private static readonly Helper _helper;

		static IntegrationPoint()
		{
			_helper = new Helper();
		}

		public static bool CreateIntegrationPoint(string integrationPointName, int sourceWorkspaceId, string fieldOverLayBehavior, bool importNativeFile, string importOverwriteMode, bool useFolderPathInformation,
			int savedSearchArtifactId)
		{
			//TODO: Get these for reals
			int targetWorkspaceId = 1119028;
			int sourceProviderArtifactId = 1039774;
			int destinationProviderArtifactId = 1039768;
			string selectedOverwrite = "AppendOverlay";
			string userName = "dnelson@kcura.com";
			string password = "Test1234!";

			DestinationConfiguration destinationConfiguration = new DestinationConfiguration() 
			{
				ArtifactTypeId = 10,
				CaseArtifactId = sourceWorkspaceId,
				FieldOverlayBehavior = fieldOverLayBehavior,
				ImportNativeFile = importNativeFile,
				ImportOverwriteMode = importOverwriteMode,
				Provider = "relativity",
				UseFolderPathInformation = useFolderPathInformation
			};

			ExportUsingSavedSearchSettings settings = new ExportUsingSavedSearchSettings
			{
				SavedSearchArtifactId = savedSearchArtifactId,
				TargetWorkspaceArtifactId = targetWorkspaceId,
				SourceWorkspaceArtifactId = sourceWorkspaceId
			};

			List<FieldMap> mapIdentifier = new List<FieldMap>
			{
				new FieldMap() {
				FieldMapType = FieldMapTypeEnum.Identifier,
				SourceField = new FieldEntry()
				{
					DisplayName = "Control Number",
					IsIdentifier = true,
					FieldIdentifier = "Control Number",
				},
				DestinationField = new FieldEntry()
				{
					DisplayName = "Control Number",
					FieldIdentifier = "1003667",
					IsIdentifier = true,
				}}
			};

			CreateIntegrationPointRequest integrationPointRequest = new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = sourceWorkspaceId,
				SourceProviderArtifactId = sourceProviderArtifactId,
				Name = integrationPointName,
				DestinationProviderArtifactId = destinationProviderArtifactId,
				SelectedOverwrite = selectedOverwrite,
				SourceConfiguration = settings,
				DestinationConfiguration = destinationConfiguration,
				FieldsMapped = mapIdentifier,
				EnableScheduler = false,
				ScheduleRule = new Scheduler()
			};

			string parameters = JsonConvert.SerializeObject(integrationPointRequest);
			string response = _helper.Rest.PostRequestAsJson("localhost", "api/kCura.IntegrationPoints.Services.IIntegrationPointsModule/Integration Point Manager/CreateIntegrationPointAsync", userName, password, false, parameters);
			//string response = _helper.Rest.PostRequestAsJson("localhost", "api/kCura.IntegrationPoints.Services.IIntegrationPointsModule/Integration Point Manager/PingAsync", userName, password, false, null);
			return true;
		}
	}
}
