using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Toggles;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ADLS;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    public class NativeFileInfoFieldsBuilderTests
    {
        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 123;

        private Mock<INativeFileRepository> _nativeFileRepositoryMock;
        private Mock<IFileLocationManager> _fileLocationManager;
        private Mock<IIsAdfTransferEnabled> _isAdfTransferEnabledMock;
        private NativeInfoFieldsBuilder _sut;

        [SetUp]
        public void SetUp()
        {
            _nativeFileRepositoryMock = new Mock<INativeFileRepository>();
            _fileLocationManager = new Mock<IFileLocationManager>();
            _isAdfTransferEnabledMock = new Mock<IIsAdfTransferEnabled>();
            _sut = new NativeInfoFieldsBuilder(_nativeFileRepositoryMock.Object, null, new EmptyLogger(), _fileLocationManager.Object, _isAdfTransferEnabledMock.Object);
        }

        [Test]
        public void ItShouldReturnBuiltColumns()
        {
            // Arrange
            const int expectedFieldCount = 5;

            // Act
            List<FieldInfoDto> result = _sut.BuildColumns().ToList();

            // Assert
            result.Count.Should().Be(expectedFieldCount);
            result.Should().Contain(info => info.DestinationFieldName == "NativeFileSize").Which.SpecialFieldType.Should().Be(SpecialFieldType.NativeFileSize);
            result.Should().Contain(info => info.DestinationFieldName == "NativeFileLocation").Which.SpecialFieldType.Should().Be(SpecialFieldType.NativeFileLocation);
            result.Should().Contain(info => info.DestinationFieldName == "NativeFileFilename").Which.SpecialFieldType.Should().Be(SpecialFieldType.NativeFileFilename);
            result.Should().Contain(info => info.SourceFieldName == "RelativityNativeType" && info.DestinationFieldName == "RelativityNativeType").Which.SpecialFieldType.Should()
                .Be(SpecialFieldType.RelativityNativeType);
            result.Should().Contain(info => info.SourceFieldName == "SupportedByViewer" && info.DestinationFieldName == "SupportedByViewer").Which.SpecialFieldType.Should()
                .Be(SpecialFieldType.SupportedByViewer);
        }

        [Test]
        public async Task GetRowValuesBuilderAsync_ShouldReturnFileInfoRowValuesBuilder()
        {
            // Act
            INativeSpecialFieldRowValuesBuilder result = await _sut.GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, Array.Empty<int>()).ConfigureAwait(false);

            // Assert
            result.Should().BeOfType<NativeInfoRowValuesBuilder>();
            _nativeFileRepositoryMock.Verify(r => r.QueryAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<ICollection<int>>()), Times.Once);
        }

        [Test]
        public async Task GetRowValuesBuilderAsync_ShouldDeduplicateNatives()
        {
            // Arrange
            const int duplicatedDocumentArtifactID = 1;
            const int notDuplicatedDocumentArtifactID = 2;

            List<INativeFile> natives = new List<INativeFile>()
            {
                new NativeFile(notDuplicatedDocumentArtifactID, string.Empty, string.Empty, 1),
                new NativeFile(duplicatedDocumentArtifactID, string.Empty, string.Empty, 1),
                new NativeFile(duplicatedDocumentArtifactID, string.Empty, string.Empty, 1),
            };

            _nativeFileRepositoryMock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<ICollection<int>>()))
                .ReturnsAsync(natives);

            // Act
            INativeSpecialFieldRowValuesBuilder result = await _sut
                .GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, Array.Empty<int>()).ConfigureAwait(false);

            // Assert
            NativeInfoRowValuesBuilder fileInfoRowValuesBuilder = result as NativeInfoRowValuesBuilder;
            fileInfoRowValuesBuilder.Should().NotBeNull();
            const int expectedNumberOfNotDuplicatedNatives = 2;
            fileInfoRowValuesBuilder.ArtifactIdToNativeFile.Count.Should().Be(expectedNumberOfNotDuplicatedNatives);
        }

        [Test]
        public async Task GetRowValuesBuilderAsync_ShouldNotLaunchADLSPathTranslation_WhenUseFMSToggleIsDisabled()
        {
            // Arrange
            _isAdfTransferEnabledMock.Setup(x => x.Value).Returns(false);

            // Act
            INativeSpecialFieldRowValuesBuilder result = await _sut
                .GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, Array.Empty<int>()).ConfigureAwait(false);

            // Assert
            _fileLocationManager.Verify(x => x.TranslateAndStoreFilePaths(It.IsAny<IDictionary<int, INativeFile>>()), Times.Never());
        }

        [Test]
        public async Task GetRowValuesBuilderAsync_ShouldAlwaysLaunchADLSPathTranslationOnce_WhenUseFMSToggleIsEnabled()
        {
            // Arrange
            _isAdfTransferEnabledMock.Setup(x => x.Value).Returns(true);

            // Act
            INativeSpecialFieldRowValuesBuilder result = await _sut
                .GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, Array.Empty<int>()).ConfigureAwait(false);

            // Assert
            _fileLocationManager.Verify(x => x.TranslateAndStoreFilePaths(It.IsAny<IDictionary<int, INativeFile>>()), Times.Once());
        }
    }
}
