using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Contracts.Models;
//using kCura.IntegrationPoints.Services;
using kCura.Relativity.Client;
using Newtonsoft.Json;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class IntegrationPoint
	{
		//public static string CreateIntegrationPoint(string integrationPointName,
		//	int sourceWorkspaceId,
		//	int targetWorkspaceId,
		//	int savedSearchArtifactId,
		//	FieldOverlayBehavior fieldOverLayBehavior,
		//	ImportOverwriteMode importOverwriteMode,
		//	bool importNativeFile,
		//	bool useFolderPathInformation,
		//	UserModel user)
		//{
		//	//TODO: Get these for reals
		//	int sourceProviderArtifactId = 1039774;
		//	int destinationProviderArtifactId = 1039768;

		//	string selectedOverwrite = null;
		//	if (importOverwriteMode.Value == ImportOverwriteMode.AppendOverlay.Value)
		//	{
		//		selectedOverwrite = "Append/Overlay";
		//	}
		//	else if (importOverwriteMode.Value == ImportOverwriteMode.Append.Value)
		//	{
		//		selectedOverwrite = "Append Only";
		//	}
		//	else if (importOverwriteMode.Value == ImportOverwriteMode.Overlay.Value)
		//	{
		//		selectedOverwrite = "Overlay Only";
		//	}

		//	DestinationConfiguration destinationConfiguration = new DestinationConfiguration
		//	{
		//		ArtifactTypeId = (int)ArtifactType.Document,
		//		CaseArtifactId = sourceWorkspaceId,
		//		FieldOverlayBehavior = fieldOverLayBehavior.Value,
		//		ImportNativeFile = importNativeFile,
		//		ImportOverwriteMode = importOverwriteMode.Value,
		//		Provider = "relativity",
		//		UseFolderPathInformation = useFolderPathInformation
		//	};

		//	ExportUsingSavedSearchSettings settings = new ExportUsingSavedSearchSettings
		//	{
		//		SavedSearchArtifactId = savedSearchArtifactId,
		//		TargetWorkspaceArtifactId = targetWorkspaceId,
		//		SourceWorkspaceArtifactId = sourceWorkspaceId
		//	};

		//	List<FieldMap> mapIdentifier = new List<FieldMap>
		//	{
		//		new FieldMap
		//		{
		//		FieldMapType = FieldMapTypeEnum.Identifier,
		//		SourceField = new FieldEntry
		//		{
		//			DisplayName = "Control Number",
		//			IsIdentifier = true,
		//			FieldIdentifier = "1003667"
		//		},
		//		DestinationField = new FieldEntry
		//		{
		//			DisplayName = "Control Number",
		//			FieldIdentifier = "1003667",
		//			IsIdentifier = true
		//		}}
		//	};

		//	CreateIntegrationPointRequest integrationPointRequest = new CreateIntegrationPointRequest
		//	{
		//		WorkspaceArtifactId = sourceWorkspaceId,
		//		SourceProviderArtifactId = sourceProviderArtifactId,
		//		Name = integrationPointName,
		//		DestinationProviderArtifactId = destinationProviderArtifactId,
		//		SelectedOverwrite = selectedOverwrite,
		//		SourceConfiguration = settings,
		//		DestinationConfiguration = destinationConfiguration,
		//		FieldsMapped = mapIdentifier,
		//		EnableScheduler = false
		//	};

		//	string parameters = JsonConvert.SerializeObject(integrationPointRequest);
		//	string request = $"{{request:{parameters}}}";
		//	string response = Rest.PostRequestAsJson("api/kCura.IntegrationPoints.Services.IIntegrationPointsModule/Integration Point Manager/CreateIntegrationPointAsync",
		//		false, user.EmailAddress, user.Password, request);
		//	return response;
		//}

		public static bool RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, UserModel user)
		{
			string request = $"{{workspaceArtifactId:{workspaceArtifactId}, integrationPointArtifactId:{integrationPointArtifactId}}}";
			Rest.PostRequestAsJson("api/kCura.IntegrationPoints.Services.IIntegrationPointsModule/Integration Point Manager/RunIntegrationPointAsync",
				false, user.EmailAddress, user.Password, request);
			return true;
		}
	}
}
