using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.EventHandlers.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class SourceConfigurationTypeOfExportUpdaterTests : TestBase
    {
        private IProviderTypeService _providerTypeService;

        public override void SetUp()
        {
            _providerTypeService = Substitute.For<IProviderTypeService>();
        }

        [TestCase(null, null, "Not null")]
        [TestCase(null, 1, "Not null")]
        [TestCase(1, null, "Not null")]
        [TestCase(1, 1, null)]
        public void GetCorrectedSourceConfiguration_ImproperArguments_ReturnsNull(int? sourceProviderId, int? destinationProviderId, string sourceConfiguration)
        {
            var sourceConfigUpdater = new SourceConfigurationTypeOfExportUpdater(_providerTypeService);

            string result = sourceConfigUpdater.GetCorrectedSourceConfiguration(sourceProviderId, destinationProviderId, sourceConfiguration);

            Assert.IsNull(result);
        }

        [TestCase(ProviderType.FTP)]
        [TestCase(ProviderType.ImportLoadFile)]
        [TestCase(ProviderType.LDAP)]
        [TestCase(ProviderType.LoadFile)]
        [TestCase(ProviderType.Other)]
        public void GetCorrectedSourceConfiguration_ProviderTypeServiceDontReturnRelativity_ReturnsNull(ProviderType providerType)
        {
            _providerTypeService.GetProviderType(Arg.Any<int>(), Arg.Any<int>()).Returns(providerType);
            var sourceConfigUpdater = new SourceConfigurationTypeOfExportUpdater(_providerTypeService);
            string sourceConfiguration = CreateSerializedSourceConfigurationToBeCorrected();

            string result = sourceConfigUpdater.GetCorrectedSourceConfiguration(1, 2, sourceConfiguration);

            Assert.IsNull(result);
        }

        [Test]
        public void GetCorrectedSourceConfiguration_UnrecognizedJsonConfig_ReturnsNull()
        {
            _providerTypeService.GetProviderType(Arg.Any<int>(), Arg.Any<int>()).Returns(ProviderType.Relativity);
            var sourceConfigUpdater = new SourceConfigurationTypeOfExportUpdater(_providerTypeService);
            var sourceConfiguration = "Not a json";

            string result = sourceConfigUpdater.GetCorrectedSourceConfiguration(1, 2, sourceConfiguration);

            Assert.IsNull(result);
        }

        [Test]
        public void GetCorrectedSourceConfiguration_NoNeedToUpdateConfiguration_ReturnsNull()
        {
            _providerTypeService.GetProviderType(Arg.Any<int>(), Arg.Any<int>()).Returns(ProviderType.Relativity);
            var sourceConfigUpdater = new SourceConfigurationTypeOfExportUpdater(_providerTypeService);
            string sourceConfiguration = CreateProperSerializedSourceConfiguration();

            string result = sourceConfigUpdater.GetCorrectedSourceConfiguration(1, 2, sourceConfiguration);

            Assert.IsNull(result);
        }

        [Test]
        public void GetCorrectedSourceConfiguration_ConfigurationNeedsUpdate_ReturnsUpdatedConfiguration()
        {
            _providerTypeService.GetProviderType(Arg.Any<int>(), Arg.Any<int>()).Returns(ProviderType.Relativity);
            var sourceConfigUpdater = new SourceConfigurationTypeOfExportUpdater(_providerTypeService);
            string sourceConfiguration = CreateSerializedSourceConfigurationToBeCorrected();

            string result = sourceConfigUpdater.GetCorrectedSourceConfiguration(1, 2, sourceConfiguration);

            Assert.That(SourceConfigurationUpdatedProperly(result));
        }

        private bool SourceConfigurationUpdatedProperly(string sourceConfigJson)
        {
            JObject jObject = JObject.Parse(sourceConfigJson);
            return jObject.SelectToken(SourceConfigurationTypeOfExportUpdater.TYPE_OF_EXPORT_NODE_NAME) != null;
        }

        private string CreateSerializedSourceConfigurationToBeCorrected()
        {
            return RemoveTypeOfExportFromSoruceConfigurationString(CreateProperSerializedSourceConfiguration());
        }

        private string CreateProperSerializedSourceConfiguration()
        {
            return JsonConvert.SerializeObject(CreateSourceConfiguration());
        }

        private SourceConfiguration CreateSourceConfiguration()
        {
            return new SourceConfiguration
            {
                FederatedInstanceArtifactId = 123,
                SavedSearch = "Saved search",
                SavedSearchArtifactId = 789,
                SourceProductionId = 456,
                SourceWorkspace = "Source workspace",
                SourceWorkspaceArtifactId = 741,
                TargetWorkspace = "Target Workspace",
                TargetWorkspaceArtifactId = 963,
                TypeOfExport = SourceConfiguration.ExportType.ProductionSet
            };
        }

        private string RemoveTypeOfExportFromSoruceConfigurationString(string sourceConfJson)
        {
            JObject jObject = JObject.Parse(sourceConfJson);
            jObject.SelectToken("TypeOfExport")?.Parent.Remove();
            return jObject.ToString();
        }
    }
}