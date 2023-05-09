using kCura.Apps.Common.Utils.Serializers;
using kCura.EDDS.WebAPI.BulkImportManagerBase;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Exceptions;
using NUnit.Framework;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
    [TestFixture, Category("Unit")]
    public class ImportSettingsTests : TestBase
    {
        [SetUp]
        public override void SetUp()
        {
        }

        [Test]
        public void ImportSettings_SerializeDeserialize()
        {
            // ARRANGE
            ISerializer serializer = new RipJsonSerializer(null);
            var settings = new DestinationConfiguration { ImportOverwriteMode = ImportOverwriteModeEnum.AppendOverlay };

            // ACT
            string serializedString = serializer.Serialize(settings);
            var deserializedSettings = serializer.Deserialize<DestinationConfiguration>(serializedString);

            // ASSERT
            Assert.IsFalse(serializedString.Contains("\"AuditLevel\""));
            Assert.IsFalse(serializedString.Contains("\"NativeFileCopyMode\""));
            Assert.IsFalse(serializedString.Contains("\"OverwriteMode\""));
            Assert.IsFalse(serializedString.Contains("\"OverlayBehavior\""));

            Assert.AreEqual(ImportOverwriteModeEnum.AppendOverlay, deserializedSettings.ImportOverwriteMode);
        }

        [TestCase(null, OverlayBehavior.UseRelativityDefaults)]
        [TestCase("", OverlayBehavior.UseRelativityDefaults)]
        [TestCase("Use Field Settings", OverlayBehavior.UseRelativityDefaults)]
        [TestCase("Merge Values", OverlayBehavior.MergeAll)]
        [TestCase("Replace Values", OverlayBehavior.ReplaceAll)]
        public void ImportSettings_ImportOverlayBehavior(string input, OverlayBehavior expectedResult)
        {
            var setting = new ImportSettings(new DestinationConfiguration { FieldOverlayBehavior = input });
            OverlayBehavior result = setting.ImportOverlayBehavior;
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void ImportSettings_ImportOverlayBehavior_Exception()
        {
            var setting = new ImportSettings(new DestinationConfiguration {FieldOverlayBehavior = "exception please" });
            Assert.That(() => setting.ImportOverlayBehavior, Throws.TypeOf<IntegrationPointsException>());
        }

        [TestCase("reLativitY")]
        public void IsRelativityProvider_ShouldReturnTrue_WhenProviderNameIsRelativity(string providerName)
        {
            var importSettings = new ImportSettings(new DestinationConfiguration { Provider = providerName });

            bool isRelativityProvider = importSettings.IsRelativityProvider();

            Assert.IsTrue(isRelativityProvider);
        }

        [TestCase("export")]
        [TestCase("ldap")]
        [TestCase("relativity!")]
        public void IsRelativityProvider_ShouldReturnFalse_WhenProviderNameIsNotRelativity(string providerName)
        {
            var importSettings = new ImportSettings(new DestinationConfiguration { Provider = providerName });

            bool isRelativityProvider = importSettings.IsRelativityProvider();

            Assert.IsFalse(isRelativityProvider);
        }
    }
}
