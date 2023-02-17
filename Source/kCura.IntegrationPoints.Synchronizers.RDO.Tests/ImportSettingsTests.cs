using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.DataReaderClient;
using NUnit.Framework;
using System.Reflection;

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
        public void ImportSettings_SerializeDesirialize()
        {
            // ARRANGE
            var serializer = new JSONSerializer();
            var settings = new ImportSettings {ImportOverwriteMode = ImportOverwriteModeEnum.AppendOverlay};

            // ACT
            string serializedString = serializer.Serialize(settings);
            serializer.Deserialize<ImportSettings>(serializedString);

            // ASSERT
            Assert.IsFalse(serializedString.Contains("\"AuditLevel\""));
            Assert.IsFalse(serializedString.Contains("\"NativeFileCopyMode\""));
            Assert.IsFalse(serializedString.Contains("\"OverwriteMode\""));
            Assert.IsFalse(serializedString.Contains("\"OverlayBehavior\""));
            Assert.AreEqual(OverwriteModeEnum.AppendOverlay, (OverwriteModeEnum)GetPropertyValue(settings, "OverwriteMode"));
        }

        [TestCase(null, ImportOverlayBehaviorEnum.UseRelativityDefaults)]
        [TestCase("", ImportOverlayBehaviorEnum.UseRelativityDefaults)]
        [TestCase("Use Field Settings", ImportOverlayBehaviorEnum.UseRelativityDefaults)]
        [TestCase("Merge Values", ImportOverlayBehaviorEnum.MergeAll)]
        [TestCase("Replace Values", ImportOverlayBehaviorEnum.ReplaceAll)]
        public void ImportSettings_ImportOverlayBehavior(string input, ImportOverlayBehaviorEnum expectedResult)
        {
            var setting = new ImportSettings {FieldOverlayBehavior = input};
            ImportOverlayBehaviorEnum result = setting.ImportOverlayBehavior;
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void ImportSettings_ImportOverlayBehavior_Exception()
        {
            var setting = new ImportSettings {FieldOverlayBehavior = "exception please"};
            Assert.That(() => setting.ImportOverlayBehavior, Throws.TypeOf<IntegrationPointsException>());
        }

        [TestCase("reLativitY")]
        public void IsRelativityProvider_ShouldReturnTrue_WhenProviderNameIsRelativity(string providerName)
        {
            var importSettings = new ImportSettings {Provider = providerName};

            bool isRelativityProvider = importSettings.IsRelativityProvider();

            Assert.IsTrue(isRelativityProvider);
        }

        [TestCase("export")]
        [TestCase("ldap")]
        [TestCase("relativity!")]
        public void IsRelativityProvider_ShouldReturnFalse_WhenProviderNameIsNotRelativity(string providerName)
        {
            var importSettings = new ImportSettings {Provider = providerName};

            bool isRelativityProvider = importSettings.IsRelativityProvider();

            Assert.IsFalse(isRelativityProvider);
        }

        private object GetPropertyValue(object o, string propertyName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            PropertyInfo property = o.GetType().GetProperty(propertyName, bindFlags);
            return property?.GetValue(o);
        }
    }
}
