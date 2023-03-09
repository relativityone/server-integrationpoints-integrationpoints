using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class ImportNativeFileCopyModeUpdaterTests : TestBase
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
        public void GetCorrectedSourceConfiguration_ImproperArguments_ReturnsNull(int? sourceProviderId, int? destinationProviderId, string configuration)
        {
            var configUpdater = new ImportNativeFileCopyModeUpdater(_providerTypeService);

            string result = configUpdater.GetCorrectedConfiguration(sourceProviderId, destinationProviderId, configuration);

            Assert.IsNull(result);
        }

        [TestCase(ProviderType.FTP, true, ImportNativeFileCopyModeEnum.CopyFiles)]
        [TestCase(ProviderType.FTP, false, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)]
        [TestCase(ProviderType.LDAP, true, ImportNativeFileCopyModeEnum.CopyFiles)]
        [TestCase(ProviderType.LDAP, false, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)]
        [TestCase(ProviderType.ImportLoadFile, true, ImportNativeFileCopyModeEnum.CopyFiles)]
        [TestCase(ProviderType.ImportLoadFile, false, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)]
        [TestCase(ProviderType.Relativity, true, ImportNativeFileCopyModeEnum.CopyFiles)]
        [TestCase(ProviderType.Relativity, false, ImportNativeFileCopyModeEnum.SetFileLinks)]
        public void GetCorrectedSourceConfiguration_SourceConfigIsMissingCopyModeField_ReturnsCorrectValueForGivenProviderType(ProviderType providerType, bool importNativeFileFlagValue, ImportNativeFileCopyModeEnum expectedResult)
        {
            _providerTypeService.GetProviderType(Arg.Any<int>(), Arg.Any<int>()).Returns(providerType);
            var configUpdater = new ImportNativeFileCopyModeUpdater(_providerTypeService);
            string configuration = CreateSerializedSourceConfigurationToBeCorrected(importNativeFileFlagValue, false);

            string updatedSourceConfig = configUpdater.GetCorrectedConfiguration(1, 2, configuration);

            Assert.AreEqual(DeserializeConfig(updatedSourceConfig).ImportNativeFileCopyMode, expectedResult);
        }

        [TestCase(ProviderType.FTP, true, ImportNativeFileCopyModeEnum.CopyFiles)]
        [TestCase(ProviderType.FTP, false, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)]
        [TestCase(ProviderType.LDAP, true, ImportNativeFileCopyModeEnum.CopyFiles)]
        [TestCase(ProviderType.LDAP, false, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)]
        [TestCase(ProviderType.ImportLoadFile, true, ImportNativeFileCopyModeEnum.CopyFiles)]
        [TestCase(ProviderType.ImportLoadFile, false, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)]
        [TestCase(ProviderType.Relativity, true, ImportNativeFileCopyModeEnum.CopyFiles)]
        [TestCase(ProviderType.Relativity, false, ImportNativeFileCopyModeEnum.SetFileLinks)]
        public void GetCorrectedSourceConfiguration_SourceConfigHasCopyModeField_ReturnsCorrectValueForGivenProviderType(ProviderType providerType, bool importNativeFileFlagValue, ImportNativeFileCopyModeEnum expectedResult)
        {
            _providerTypeService.GetProviderType(Arg.Any<int>(), Arg.Any<int>()).Returns(providerType);
            var configUpdater = new ImportNativeFileCopyModeUpdater(_providerTypeService);
            string configuration = CreateSerializedSourceConfigurationToBeCorrected(importNativeFileFlagValue, true);

            string updatedSourceConfig = configUpdater.GetCorrectedConfiguration(1, 2, configuration);

            Assert.AreEqual(DeserializeConfig(updatedSourceConfig).ImportNativeFileCopyMode, expectedResult);
        }

        [TestCase(ProviderType.LoadFile, true)]
        [TestCase(ProviderType.LoadFile, false)]
        [TestCase(ProviderType.Other, true)]
        [TestCase(ProviderType.Other, false)]
        public void GetCorrectedSourceConfiguration_ProviderTypeDoesntNeedModification_ReturnsNull(ProviderType providerType, bool shouldHaveImportNativeFileCopyMode)
        {
            _providerTypeService.GetProviderType(Arg.Any<int>(), Arg.Any<int>()).Returns(providerType);
            var configUpdater = new ImportNativeFileCopyModeUpdater(_providerTypeService);
            string configuration = CreateSerializedSourceConfigurationToBeCorrected(true, shouldHaveImportNativeFileCopyMode);

            string updatedSourceConfig = configUpdater.GetCorrectedConfiguration(1, 2, configuration);

            Assert.IsNull(updatedSourceConfig);
        }

        private string CreateSerializedSourceConfigurationToBeCorrected(bool importNativeFileFlagValue, bool shouldHaveImportNativeFileCopyMode)
        {
            string config = CreateProperSerializedSourceConfiguration(importNativeFileFlagValue);
            return shouldHaveImportNativeFileCopyMode
                ? config
                : RemoveImportNativeFileCopyModeFromSoruceConfigurationString(config);
        }

        private string CreateProperSerializedSourceConfiguration(bool importNativeFileFlagValue)
        {
            return JsonConvert.SerializeObject(CreateSourceConfiguration(importNativeFileFlagValue));
        }

        private ImportSettings CreateSourceConfiguration(bool importNativeFileFlagValue)
        {
            return new ImportSettings
            {
                ImportNativeFileCopyMode = (ImportNativeFileCopyModeEnum)int.MinValue,
                ImportNativeFile = importNativeFileFlagValue
            };
        }

        private string RemoveImportNativeFileCopyModeFromSoruceConfigurationString(string sourceConfJson)
        {
            JObject jObject = JObject.Parse(sourceConfJson);
            jObject.SelectToken("ImportNativeFileCopyMode")?.Parent.Remove();
            return jObject.ToString();
        }

        private ImportSettings DeserializeConfig(string sourceConfigurationString)
        {
            return JsonConvert.DeserializeObject<ImportSettings>(sourceConfigurationString);
        }
    }
}
