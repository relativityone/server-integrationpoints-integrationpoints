using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ObjectQuery;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoint.Tests.Core
{
	[TestFixture]
	public class SampleTest : WorkspaceDependentTemplate 
	{
		public SampleTest()
			: base("WorkspaceA", "WorkspaceB")
		{
		}

		[Test]
		[Explicit]
		public void TestUser()
		{
			bool createdUser = User.CreateUserRest("first", "last", "flast@kcura.com");
		}

		[Test]
		[Explicit]
		public void TestIntegrationPoint()
		{
			//IIntegrationPointService service = Container.Resolve<IIntegrationPointService>();
			////TODO: Get these for reals
			//int sourceWorkspaceId = 1118254;
			//int targetWorkspaceId = 1119028;
			//int sourceProviderArtifactId = 1039774;
			//int destinationProviderArtifactId = 1039768;
			//string fieldOverlayBehavior = "Use Field Settings";
			//string importOverwriteMode = "AppendOverlay";
			//bool importNativeFile = false;
			//bool useFolderPathInformation = false;
			//string selectedOverwrite = "Append/Overlay";
			//string userName = "relativity.admin@kcura.com";
			//string password = "Test1234!";
			//int savedSearchArtifactId = 1039795;

			//DestinationConfiguration destinationConfiguration = new DestinationConfiguration()
			//{
			//	ArtifactTypeId = 10,
			//	CaseArtifactId = sourceWorkspaceId,
			//	FieldOverlayBehavior = fieldOverlayBehavior,
			//	ImportNativeFile = importNativeFile,
			//	ImportOverwriteMode = importOverwriteMode,
			//	Provider = "relativity",
			//	UseFolderPathInformation = useFolderPathInformation
			//};

			//ExportUsingSavedSearchSettings settings = new ExportUsingSavedSearchSettings
			//{
			//	SavedSearchArtifactId = savedSearchArtifactId,
			//	TargetWorkspaceArtifactId = targetWorkspaceId,
			//	SourceWorkspaceArtifactId = sourceWorkspaceId
			//};

			//List<FieldMap> mapIdentifier = new List<FieldMap>
			//{
			//	new FieldMap() {
			//	FieldMapType = FieldMapTypeEnum.Identifier,
			//	SourceField = new FieldEntry()
			//	{
			//		DisplayName = "Control Number",
			//		IsIdentifier = true,
			//		FieldIdentifier = "Control Number",
			//	},
			//	DestinationField = new FieldEntry()
			//	{
			//		DisplayName = "Control Number",
			//		FieldIdentifier = "1003667",
			//		IsIdentifier = true,
			//	}}
			//};

			//IntegrationModel model = new IntegrationModel()
			//{
			//	Name = "My little integration point",
			//	SelectedOverwrite = selectedOverwrite,
			//	SourceProvider = sourceProviderArtifactId,
			//	DestinationProvider = destinationProviderArtifactId,
			//	Destination = JsonConvert.SerializeObject(destinationConfiguration),
			//	Scheduler = new Scheduler(),
			//	NextRun = null,
			//	LastRun = null,
			//	SourceConfiguration = JsonConvert.SerializeObject(settings),
			//	Map = JsonConvert.SerializeObject(mapIdentifier),
			//	LogErrors = true,
			//	HasErrors = null,
			//	NotificationEmails = String.Empty
			//};
			//service.SaveIntegration(model);

			bool createdIntegrationPoint = IntegrationPoint.CreateIntegrationPoint("My little integration point", 1118254, "Use Field Settings", false, "AppendOverlay", false, 1039795);
		}
	}
}