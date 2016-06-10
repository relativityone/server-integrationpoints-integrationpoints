using System;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Category("Integration Tests")]
	public class IntegrationPointServiceTests : WorkspaceDependentTemplate
	{
		private const string _SOURCECONFIG = "Source Config";
		private const string _NAME = "Name";
		private const string _FIELDMAP = "Map";
		private DestinationProvider _destinationProvider;

		public IntegrationPointServiceTests()
			: base("IntegrationPointService Source", null)
		{
		}

		[TestFixtureSetUp]
		public override void SetUp()
		{
			base.SetUp();
			_destinationProvider = CaseContext.RsapiService.DestinationProviderLibrary.ReadAll().First();
		}

		#region UpdateProperties

		[Test]
		public void UpdateNothing()
		{
			const string name = "Resaved Rip";
			IntegrationModel modelToUse = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = CreateOrUpdateIntegrationPoint(modelToUse);
			IntegrationModel newModel = CreateOrUpdateIntegrationPoint(defaultModel);

			ValidateModel(defaultModel, newModel, new string[0]);
		}

		[Test]
		public void UpdateName_OnRanIp_ErrorCase()
		{
			const string name = "Update Name - OnRanIp";
			IntegrationModel modelToUse = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = CreateOrUpdateIntegrationPoint(modelToUse);

			defaultModel.Name = "newName";

			Assert.Throws<Exception>(() => CreateOrUpdateIntegrationPoint(defaultModel));
		}

		[Test]
		public void UpdateMap_OnRanIp()
		{
			const string name = "Update Map - OnRanIp";
			IntegrationModel modelToUse = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = CreateOrUpdateIntegrationPoint(modelToUse);

			defaultModel.Map = "Blahh";

			IntegrationModel newModel = CreateOrUpdateIntegrationPoint(defaultModel);
			ValidateModel(defaultModel, newModel, new string[] { _FIELDMAP });
		}

		[Test]
		public void UpdateConfig_OnNewRip()
		{
			const string name = "Update Source Config - SavedSearch - OnNewRip";
			IntegrationModel modelToUse = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = CreateOrUpdateIntegrationPoint(modelToUse);

			int newSavedSearch = SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactId, name);
			defaultModel.SourceConfiguration = CreateSourceConfig(newSavedSearch, SourceWorkspaceArtifactId);

			Assert.Throws<Exception>(() => CreateOrUpdateIntegrationPoint(defaultModel), "Unable to save Integration Point: Source Configuration cannot be changed once the Integration Point has been run");
		}

		[Test]
		public void UpdateName_OnNewRip()
		{
			const string name = "Update Name - OnNewRip";
			IntegrationModel modelToUse = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = CreateOrUpdateIntegrationPoint(modelToUse);

			defaultModel.Name = name + " 2";

			Assert.Throws<Exception>(() => CreateOrUpdateIntegrationPoint(defaultModel), "Unable to save Integration Point: Name cannot be changed once the Integration Point has been run");
		}

		[Test]
		public void UpdateMap_OnNewRip()
		{
			const string name = "Update Map - OnNewRip";
			IntegrationModel modelToUse = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = CreateOrUpdateIntegrationPoint(modelToUse);

			defaultModel.Map = "New Map string";

			IntegrationModel newModel = CreateOrUpdateIntegrationPoint(defaultModel);

			ValidateModel(defaultModel, newModel, new[] { _FIELDMAP });
		}

		#endregion


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
			Assert.AreEqual(expectedModel.LastRun, actual.LastRun);
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
			return $"{{\"SavedSearchArtifactId\":{savedSearchId},\"SourceWorkspaceArtifactId\":\"{SourceWorkspaceArtifactId}\",\"TargetWorkspaceArtifactId\":{targetWorkspaceId}}}";
		}

		private IntegrationModel CreateIntegrationPointThatHasNotRun(string name)
		{
			return new IntegrationModel()
			{
				Destination = $"{{\"artifactTypeID\":10,\"CaseArtifactId\":{TargetWorkspaceArtifactId},\"Provider\":\"relativity\",\"DoNotUseFieldsMapCache\":true,\"ImportOverwriteMode\":\"AppendOnly\",\"importNativeFile\":\"false\",\"UseFolderPathInformation\":\"false\",\"ExtractedTextFieldContainsFilePath\":\"false\",\"ExtractedTextFileEncoding\":\"utf - 16\",\"CustodianManagerFieldContainsLink\":\"true\",\"FieldOverlayBehavior\":\"Use Field Settings\"}}",
				DestinationProvider = _destinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
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
	}
}