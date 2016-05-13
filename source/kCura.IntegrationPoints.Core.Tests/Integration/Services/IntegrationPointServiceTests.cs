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
			Helper.PermissionManager.UserCanEditDocuments(SourecWorkspaceArtifactId).Returns(true);
			Helper.PermissionManager.UserCanImport(TargetWorkspaceArtifactId).Returns(true);
			Helper.PermissionManager.UserCanViewArtifact(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);

			IIntegrationPointService service = Container.Resolve<IIntegrationPointService>();
			ICaseServiceContext caseContext = Container.Resolve<ICaseServiceContext>();
			IEnumerable<SourceProvider> providers = caseContext.RsapiService.SourceProviderLibrary.ReadAll(Guid.Parse(SourceProviderFieldGuids.Name), Guid.Parse(Data.SourceProviderFieldGuids.Identifier));
			SourceProvider relativityProvider = providers.First(provider => provider.Name == "Relativity");
			DestinationProvider destinationProvider = caseContext.RsapiService.DestinationProviderLibrary.ReadAll().First();

			IntegrationModel model = new IntegrationModel()
			{
				Destination = $"{{\"artifactTypeID\":10,\"CaseArtifactId\":{TargetWorkspaceArtifactId},\"Provider\":\"relativity\",\"DoNotUseFieldsMapCache\":true,\"ImportOverwriteMode\":\"AppendOnly\",\"importNativeFile\":\"false\",\"UseFolderPathInformation\":\"false\",\"ExtractedTextFieldContainsFilePath\":\"false\",\"ExtractedTextFileEncoding\":\"utf - 16\",\"CustodianManagerFieldContainsLink\":\"true\",\"FieldOverlayBehavior\":\"Use Field Settings\"}}",
				DestinationProvider = destinationProvider.ArtifactId,
				SourceProvider = relativityProvider.ArtifactId,
				SourceConfiguration = $"{{\"SavedSearchArtifactId\":{SavedSearchArtifactId},\"SourceWorkspaceArtifactId\":\"{SourecWorkspaceArtifactId}\",\"TargetWorkspaceArtifactId\":{TargetWorkspaceArtifactId}}}",
				LogErrors = true,
				Name = $"Sample Object - {DateTime.Today}",
				Map = String.Empty,
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler(),
			};

			int integrationPointAritfactId = service.SaveIntegration(model);
			Data.IntegrationPoint integrationPoint = caseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointAritfactId);
			Assert.IsNotNull(integrationPoint);
		}
	}
}