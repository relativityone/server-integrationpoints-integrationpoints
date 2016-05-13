using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core;
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
		private SourceProvider _relativityProvider;
		private DestinationProvider _destinationProvider;

		public IntegrationPointServiceTests()
			: base("IntegrationPointService Source", null)
		{
		}

		[TestFixtureSetUp]
		public override void SetUp()
		{
			base.SetUp();
			ICaseServiceContext caseContext = Container.Resolve<ICaseServiceContext>();
			IEnumerable<SourceProvider> providers =
				caseContext.RsapiService.SourceProviderLibrary.ReadAll(Guid.Parse(SourceProviderFieldGuids.Name),
					Guid.Parse(Data.SourceProviderFieldGuids.Identifier));


			_relativityProvider = providers.First(provider => provider.Name == "Relativity");
			_destinationProvider = caseContext.RsapiService.DestinationProviderLibrary.ReadAll().First();

		}

		[Test]
		public void UpdateNothing()
		{
			const string name = "Resaved Rip";
			IntegrationModel modelToUse = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = SaveModel(modelToUse);
			IntegrationModel newModel = SaveModel(defaultModel);

			ValidateModel(defaultModel, newModel, new string[0]);
		}

		[Test]
		public void UpdateName_OnRanIp()
		{
			const string name = "Update Name - OnRanIp";
			IntegrationModel modelToUse = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = SaveModel(modelToUse);

			defaultModel.Name = "newName";

			IntegrationModel newModel = SaveModel(defaultModel);
			ValidateModel(defaultModel, newModel, new[] { _NAME });
		}

		[Test]
		public void UpdateMap_OnRanIp()
		{
			const string name = "Update Map - OnRanIp";
			IntegrationModel modelToUse = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = SaveModel(modelToUse);

			defaultModel.Map = "Blahh";

			IntegrationModel newModel = SaveModel(defaultModel);
			ValidateModel(defaultModel, newModel, new string[] { _FIELDMAP });
		}

		[Test]
		public void UpdateConfig_OnNewRip()
		{
			const string name = "Update Source Config - SavedSearch - OnNewRip";
			IntegrationModel modelToUse = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = SaveModel(modelToUse);

			int newSavedSearch = SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactId, name);
			defaultModel.SourceConfiguration = CreateSourceConfig(newSavedSearch, SourceWorkspaceArtifactId);

			IntegrationModel newModel = SaveModel(defaultModel);

			ValidateModel(defaultModel, newModel, new []{ _SOURCECONFIG });
		}

		[Test]
		public void UpdateName_OnNewRip()
		{
			const string name = "Update Name - OnNewRip";
			IntegrationModel modelToUse = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = SaveModel(modelToUse);

			defaultModel.Name = name + " 2";

			IntegrationModel newModel = SaveModel(defaultModel);

			ValidateModel(defaultModel, newModel, new[] { _NAME });
		}

		[Test]
		public void UpdateMap_OnNewRip()
		{
			const string name = "Update Map - OnNewRip";
			IntegrationModel modelToUse = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = SaveModel(modelToUse);

			defaultModel.Map = "New Map string";

			IntegrationModel newModel = SaveModel(defaultModel);

			ValidateModel(defaultModel, newModel, new[] { _FIELDMAP });
		}


		private const string _SOURCECONFIG = "Source Config";
		private const string _NAME = "Name";
		private const string _FIELDMAP = "Map";


		private void ValidateModel(IntegrationModel expectedModel, IntegrationModel actual, string[] updatedProperties)
		{
			Action<object, object> assertion = DetermineAssertion(updatedProperties, _SOURCECONFIG);
			assertion(expectedModel.SourceConfiguration, actual.SourceConfiguration);

			assertion = DetermineAssertion(updatedProperties, _NAME);
			assertion(expectedModel.Name, actual.Name);

			assertion = DetermineAssertion(updatedProperties, _FIELDMAP);
			assertion(expectedModel.Map, actual.Map);

			Assert.AreEqual(expectedModel.HasErrors, actual.HasErrors);
			Assert.AreEqual(expectedModel.ArtifactID, actual.ArtifactID);
			Assert.AreEqual(expectedModel.DestinationProvider, actual.DestinationProvider);
		}

		private Action<object, object> DetermineAssertion(string[] updatedProperties, string property)
		{
			Action<object, object> assertion;
			if (updatedProperties.Contains(property))
			{
				assertion = Assert.AreNotEqual;
			}
			else
			{
				assertion = Assert.AreEqual;
			}
			return assertion;
		}

		private string CreateSourceConfig(int savedSearchId, int targetWorkspaceId)
		{
			return$"{{\"SavedSearchArtifactId\":{savedSearchId},\"SourceWorkspaceArtifactId\":\"{SourceWorkspaceArtifactId}\",\"TargetWorkspaceArtifactId\":{targetWorkspaceId}}}";
		}

		private IntegrationModel CreateIntegrationPointThatHasNotRun(string name)
		{
			return new IntegrationModel()
			{
				Destination = $"{{\"artifactTypeID\":10,\"CaseArtifactId\":{TargetWorkspaceArtifactId},\"Provider\":\"relativity\",\"DoNotUseFieldsMapCache\":true,\"ImportOverwriteMode\":\"AppendOnly\",\"importNativeFile\":\"false\",\"UseFolderPathInformation\":\"false\",\"ExtractedTextFieldContainsFilePath\":\"false\",\"ExtractedTextFileEncoding\":\"utf - 16\",\"CustodianManagerFieldContainsLink\":\"true\",\"FieldOverlayBehavior\":\"Use Field Settings\"}}",
				DestinationProvider = _destinationProvider.ArtifactId,
				SourceProvider = _relativityProvider.ArtifactId,
				SourceConfiguration = CreateSourceConfig(SavedSearchArtifactId, TargetWorkspaceArtifactId),
				LogErrors = true,
				Name = $"${name} - {DateTime.Today}",
				Map = "[]",
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler(),
			};
		}



		private IntegrationModel CreateIntegrationPointThatIsAlreadyRunModel(string name)
		{
			IntegrationModel model = CreateIntegrationPointThatHasNotRun(name);
			model.LastRun = DateTime.Now;
			return model;
		}


		private IntegrationModel SaveModel(IntegrationModel model)
		{
			Helper.PermissionManager.UserCanEditDocuments(SourceWorkspaceArtifactId).Returns(true);
			Helper.PermissionManager.UserCanImport(TargetWorkspaceArtifactId).Returns(true);
			Helper.PermissionManager.UserCanViewArtifact(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);

			IIntegrationPointService service = Container.Resolve<IIntegrationPointService>();

			int integrationPointAritfactId = service.SaveIntegration(model);

			var rdo = service.GetRdo(integrationPointAritfactId);
			IntegrationModel newModel = new IntegrationModel(rdo);
			return newModel;
		}
	}
}