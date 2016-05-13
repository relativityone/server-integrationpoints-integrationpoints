using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using kCura.Data.RowDataGateway;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Explicit]
	public class IntegrationPointServiceTests : WorkspaceDependentTemplate
	{
		public IntegrationPointServiceTests()
			: base("IntegrationPointService Source", null)
		{
		}

		[Test]
		[Explicit]
		public void CreateIntegrationPoint_GoldFlow()
		{
			ICaseServiceContext caseContext = Container.Resolve<ICaseServiceContext>();
			IntegrationModel defaultModel = SaveModel(null);
			Data.IntegrationPoint integrationPoint = caseContext.RsapiService.IntegrationPointLibrary.Read(defaultModel.ArtifactID);
			Assert.IsNotNull(integrationPoint);
		}

		[Test]
		[Explicit]
		public void UpdateName()
		{
			IntegrationModel defaultModel = SaveModel(null);

			defaultModel.Name = "newName";

			IntegrationModel newModel = SaveModel(defaultModel);
			Assert.AreNotEqual(defaultModel.Name, newModel.Name);
			Assert.AreEqual(defaultModel.ArtifactID, newModel.ArtifactID);
			Assert.AreEqual(defaultModel.Map, newModel.Map);
			Assert.AreEqual(defaultModel.SourceProvider, newModel.SourceProvider);
			Assert.AreEqual(defaultModel.DestinationProvider, newModel.DestinationProvider);
			Assert.AreEqual(defaultModel.HasErrors, newModel.HasErrors);
			Assert.AreEqual(defaultModel.SourceConfiguration, newModel.SourceConfiguration);
		}

		[Test]
		[Explicit]
		public void UpdateMap()
		{
			IntegrationModel defaultModel = SaveModel(null);

			defaultModel.Map = "Blahh";

			IntegrationModel newModel = SaveModel(defaultModel);
			Assert.AreEqual(defaultModel.Name, newModel.Name);
			Assert.AreEqual(defaultModel.ArtifactID, newModel.ArtifactID);
			Assert.AreNotEqual(defaultModel.Map, newModel.Map);
			Assert.AreEqual(defaultModel.SourceProvider, newModel.SourceProvider);
			Assert.AreEqual(defaultModel.DestinationProvider, newModel.DestinationProvider);
			Assert.AreEqual(defaultModel.HasErrors, newModel.HasErrors);
			Assert.AreEqual(defaultModel.SourceConfiguration, newModel.SourceConfiguration);
		}

		private IntegrationModel CreateDefaultModel(int sourceProvider, int destinationProvider)
		{
			return new IntegrationModel()
			{
				Destination = $"{{\"artifactTypeID\":10,\"CaseArtifactId\":{TargetWorkspaceArtifactId},\"Provider\":\"relativity\",\"DoNotUseFieldsMapCache\":true,\"ImportOverwriteMode\":\"AppendOnly\",\"importNativeFile\":\"false\",\"UseFolderPathInformation\":\"false\",\"ExtractedTextFieldContainsFilePath\":\"false\",\"ExtractedTextFileEncoding\":\"utf - 16\",\"CustodianManagerFieldContainsLink\":\"true\",\"FieldOverlayBehavior\":\"Use Field Settings\"}}",
				DestinationProvider = destinationProvider,
				SourceProvider = sourceProvider,
				SourceConfiguration = $"{{\"SavedSearchArtifactId\":{SavedSearchArtifactId},\"SourceWorkspaceArtifactId\":\"{SourceWorkspaceArtifactId}\",\"TargetWorkspaceArtifactId\":{TargetWorkspaceArtifactId}}}",
				LogErrors = true,
				Name = $"Sample Object - {DateTime.Today}",
				Map = String.Empty,
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler(),
			};
		}


		private IntegrationModel SaveModel(IntegrationModel model)
		{
			Helper.PermissionManager.UserCanEditDocuments(SourceWorkspaceArtifactId).Returns(true);
			Helper.PermissionManager.UserCanImport(TargetWorkspaceArtifactId).Returns(true);
			Helper.PermissionManager.UserCanViewArtifact(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);

			IIntegrationPointService service = Container.Resolve<IIntegrationPointService>();
			ICaseServiceContext caseContext = Container.Resolve<ICaseServiceContext>();
			IEnumerable<SourceProvider> providers = caseContext.RsapiService.SourceProviderLibrary.ReadAll(Guid.Parse(SourceProviderFieldGuids.Name), Guid.Parse(Data.SourceProviderFieldGuids.Identifier));

			if (model == null)
			{
				SourceProvider relativityProvider = providers.First(provider => provider.Name == "Relativity");
				DestinationProvider destinationProvider = caseContext.RsapiService.DestinationProviderLibrary.ReadAll().First();
				model = CreateDefaultModel(relativityProvider.ArtifactId, destinationProvider.ArtifactId);
			}

			int integrationPointAritfactId = service.SaveIntegration(model);

			var rdo = service.GetRdo(integrationPointAritfactId);
			IntegrationModel newModel = new IntegrationModel(rdo);
			return newModel;
		}
	}
}