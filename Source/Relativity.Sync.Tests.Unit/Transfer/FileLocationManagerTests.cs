using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal sealed class FileLocationManagerTests
    {
        private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 123456;
        private const string _EXAMPLE_SERVER_PATH = @"\\files.T00.ctus000001.r1.kcura.com\T00";

        private Mock<ISynchronizationConfiguration> _configurationMock;
        private IFileLocationManager _sut;

        [SetUp]
        public void SetUp()
        {
            _configurationMock = new Mock<ISynchronizationConfiguration>();
            _configurationMock.Setup(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ARTIFACT_ID);

            _sut = new FileLocationManager(new EmptyLogger(), _configurationMock.Object);
        }

        [Test]
        public void TranslateAndStoreFilePaths_ShouldCreateCorrectNumberOfDistinctFmsBatches()
        {
            // Arrange
            IDictionary<int, INativeFile> testNativesSet = PrepareTestNativeFilesSet();
            int expectedNumberOfFmsBatches = 4;

            // Act
            _sut.TranslateAndStoreFilePaths(testNativesSet);
            IList<FmsBatchInfo> result = _sut.GetStoredLocations();

            // Assert
            result.Count.Should().Be(expectedNumberOfFmsBatches);
            result.Should().OnlyHaveUniqueItems(x => x.SourceLocationShortPath);
            result.Should().OnlyHaveUniqueItems(x => x.DestinationLocationShortPath);
            result.Select(x => x.TraceId).Distinct().Count().Should().Be(1);
        }

        [Test]
        public void TranslateAndStoreFilePaths_ShouldCreateCorrectPathsForFmsBatch()
        {
            // Arrange
            IDictionary<int, INativeFile> testInput = new Dictionary<int, INativeFile>
            {
                {
                    1, new NativeFile(1, $@"{_EXAMPLE_SERVER_PATH}\EDDS1018439\RV_1\5dcd0d81-13f0-4194-9a11-4a56c0fd8159", string.Empty, 1)
                }
            };
            string expectedSourceShortPath = @"EDDS1018439/RV_1";
            int expectedNumberOfFilesWithinBatch = 1;

            // Act
            _sut.TranslateAndStoreFilePaths(testInput);
            IList<FmsBatchInfo> result = _sut.GetStoredLocations();

            // Assert
            result.Count.Should().Be(1);
            FmsBatchInfo singleBatch = result.Single();

            string expectedDestinationShortPath = $@"Files/EDDS{_DESTINATION_WORKSPACE_ARTIFACT_ID}/RV_{result.Select(x => x.BatchId).First()}";
            string expectedLinkForIAPI = $@"{_EXAMPLE_SERVER_PATH}\{expectedDestinationShortPath}\5dcd0d81-13f0-4194-9a11-4a56c0fd8159".Replace("/", @"\");

            singleBatch.SourceLocationShortPath.Should().Be(expectedSourceShortPath);
            singleBatch.DestinationLocationShortPath.Should().StartWith(expectedDestinationShortPath);
            singleBatch.Files.Count.Should().Be(expectedNumberOfFilesWithinBatch);
            singleBatch.Files.First().LinkForIAPI.Should().Be(expectedLinkForIAPI);

            testInput.First().Value.Location.Should().Be(expectedLinkForIAPI);
        }

        private IDictionary<int, INativeFile> PrepareTestNativeFilesSet()
        {
            IDictionary<int, INativeFile> testNatives = new Dictionary<int, INativeFile>();
            List<string> testLocations = new List<string>
            {
                $@"{_EXAMPLE_SERVER_PATH}\EDDS1018439\RV_1\5dcd0d81-13f0-4194-9a11-4a56c0fd8159",
                $@"{_EXAMPLE_SERVER_PATH}\EDDS1018439\RV_1\dd370238-83ef-4071-9fec-7b2954d89aee",
                $@"{_EXAMPLE_SERVER_PATH}\EDDS1018439\RV_1\InnerFolderName\5dcd0d81-13f0-4194-9a11-4a56c0fd8159",
                $@"{_EXAMPLE_SERVER_PATH}\EDDS1018439\RV_1\InnerFolderName\6cfa4b7e-f281-4ed2-9948-2fb2061c4317",
                $@"{_EXAMPLE_SERVER_PATH}\EDDS1018439\RV_2\c8b95c8a-dfa3-4660-96ea-8f1dd26bf343",
                $@"{_EXAMPLE_SERVER_PATH}\EDDS1018439\RV_3\h4s17n8u-25zi-4310-86by-1f1rr26bf12r",
                string.Empty,
                null
            };

            for (int i = 0; i < testLocations.Count; i++)
            {
                testNatives.Add(i + 1, new NativeFile(i + 1, testLocations[i], $"testFile{i}.txt", 1));
            }

            return testNatives;
        }
    }
}
