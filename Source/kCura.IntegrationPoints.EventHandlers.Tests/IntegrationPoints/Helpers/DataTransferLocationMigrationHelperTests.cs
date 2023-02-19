using System.Collections.Generic;
using System.IO;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
    [TestFixture, Category("Unit")]
    public class DataTransferLocationMigrationHelperTests : TestBase
    {
        private const string SOURCECONFIGURATION_FILESHARE_KEY = "Fileshare";
        private DataTransferLocationMigrationHelper _dataTransferLocationMigrationHelper;
        private ISerializer _serializer;
        private string _newDataTransferLocationRoot = "DataTransfer\\Export";

        public override void SetUp()
        {
            _serializer = new JSONSerializer();
            _dataTransferLocationMigrationHelper = new DataTransferLocationMigrationHelper(_serializer);
        }

        [Test]
        [TestCaseSource(nameof(GetUpdatedSourceConfigurationTestData))]
        public void ItShouldGetUpdatedSourceConfiguration(string sourceConfiguration, string relativeExportLocation)
        {
            IList<string> testProcessingSourceLocations = new List<string>()
            {
                "ProcessingSourceLocation\\Export",
                "ExportSourceLocation"
            };

            string result = _dataTransferLocationMigrationHelper.GetUpdatedSourceConfiguration(sourceConfiguration,
                    testProcessingSourceLocations, _newDataTransferLocationRoot);
            Dictionary<string, object> deserializedResult = _serializer.Deserialize<Dictionary<string, object>>(result);

            string expectedResult = Path.Combine(_newDataTransferLocationRoot, relativeExportLocation);

            Assert.That(deserializedResult[SOURCECONFIGURATION_FILESHARE_KEY], Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldGetUpdatedSourceConfigurationWithNoProcessingSourceLocations()
        {
            string sourceConfiguration = @"{'Fileshare':'ProcessingSourceLocation\\Export'}";
            string result = _dataTransferLocationMigrationHelper.GetUpdatedSourceConfiguration(sourceConfiguration,
                new List<string>(), _newDataTransferLocationRoot);
            Dictionary<string, object> deserializedResult = _serializer.Deserialize<Dictionary<string, object>>(result);

            string expectedResult = _newDataTransferLocationRoot;

            Assert.That(deserializedResult[SOURCECONFIGURATION_FILESHARE_KEY], Is.EqualTo(expectedResult));
        }

        private static IEnumerable<TestCaseData> GetUpdatedSourceConfigurationTestData()
        {
            yield return new TestCaseData(@"{'Fileshare':'\\ProcessingSourceLocation\\Export'}", string.Empty);
            yield return new TestCaseData(@"{'Fileshare':'ProcessingSourceLocation\\Export'}", string.Empty);
            yield return new TestCaseData(@"{'Fileshare':'ProcessingSourceLocation\\Export\\'}", string.Empty);
            yield return new TestCaseData(@"{'Fileshare':'ProcessingSourceLocation\\Export\\ExportFolder'}", "ExportFolder");
            yield return new TestCaseData(@"{'Fileshare':'ProcessingSourceLocation\\Export\\ExportFolder\\ExportChildFolder'}",
                    "ExportFolder\\ExportChildFolder");
            yield return new TestCaseData(@"{'Fileshare':'\\Export'}", string.Empty);
            yield return new TestCaseData(@"{'Fileshare':'Export'}", string.Empty);
            yield return new TestCaseData(@"{'Fileshare':'Export\\ExportFolder'}", string.Empty);
            yield return new TestCaseData(@"{'Fileshare':'ExportFolder'}", string.Empty);
            yield return new TestCaseData(@"{'Fileshare':'ExportSourceLocation'}", string.Empty);
            yield return new TestCaseData(@"{'Fileshare':'ExportSourceLocation\\Export'}", "Export");
            yield return new TestCaseData(@"{'Fileshare':'ExportSourceLocation\\ExportParent\\ExportChild'}", "ExportParent\\ExportChild");
            yield return new TestCaseData(@"{'Fileshare':''}", string.Empty);
            yield return new TestCaseData(@"{'Fileshare':'\\'}", string.Empty);
            yield return new TestCaseData(@"{'Fileshare':'\\Export'}", string.Empty);
        }
    }
}
