using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    public class ImageInfoFieldsBuilderTests
    {
        private const int _SOURCE_WORKSPACE_ID = 1;
        private readonly int[] _DOCUMENT_ARTIFACT_IDS = new int[] { 1, 2, 3 };

        private ImageInfoFieldsBuilder _sut;

        private Mock<IImageFileRepository> _imageFileRepositoryMock;
        private Mock<IImageRetrieveConfiguration> _configurationFake;
        private Mock<IAPILog> _syncLogMock;

        [SetUp]
        public void SetUp()
        {
            _imageFileRepositoryMock = new Mock<IImageFileRepository>();
            _imageFileRepositoryMock.Setup(x => x.QueryImagesForDocumentsAsync(_SOURCE_WORKSPACE_ID, _DOCUMENT_ARTIFACT_IDS, It.IsAny<QueryImagesOptions>()))
                .ReturnsAsync(Enumerable.Empty<ImageFile>());

            _configurationFake = new Mock<IImageRetrieveConfiguration>();

            _syncLogMock = new Mock<IAPILog>();

            _sut = new ImageInfoFieldsBuilder(
                _imageFileRepositoryMock.Object,
                _configurationFake.Object,
                null,
                _syncLogMock.Object);
        }

        [Test]
        public void BuildColumns_ShouldReturnAllRequiredFieldTypes()
        {
            // Arrange
            var expectedFieldTypes = new SpecialFieldType[]
            {
                SpecialFieldType.ImageFileName,
                SpecialFieldType.ImageFileLocation,
                SpecialFieldType.ImageIdentifier
            };

            // Act
            var result = _sut.BuildColumns();

            // Assert
            result.Select(x => x.SpecialFieldType).Should().BeEquivalentTo(expectedFieldTypes);
        }

        [Test]
        public async Task GetRowValuesBuilderAsync_ShouldReturnImageInfoRowValuesBuilderType()
        {
            // Act
            var result = await _sut.GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ID, _DOCUMENT_ARTIFACT_IDS).ConfigureAwait(false);

            // Assert
            result.Should().BeOfType<ImageInfoRowValuesBuilder>();
        }

        [Test]
        public async Task GetRowValuesBuilderAsync_ShouldInvokeImageRetrieviengWithProperParameters()
        {
            // Arrange
            var expectedProductionIds = new int[] { 10, 20, 30 };
            var expectedIncludeOriginalImageIfNotFoundInProductions = true;

            _configurationFake.Setup(x => x.ProductionImagePrecedence).Returns(expectedProductionIds);
            _configurationFake.Setup(x => x.IncludeOriginalImageIfNotFoundInProductions).Returns(expectedIncludeOriginalImageIfNotFoundInProductions);

            // Act
            await _sut.GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ID, _DOCUMENT_ARTIFACT_IDS).ConfigureAwait(false);

            // Assert
            _imageFileRepositoryMock.Verify(
                x => x.QueryImagesForDocumentsAsync(
                    _SOURCE_WORKSPACE_ID,
                    _DOCUMENT_ARTIFACT_IDS,
                    It.Is<QueryImagesOptions>(q =>
                        q.IncludeOriginalImageIfNotFoundInProductions == expectedIncludeOriginalImageIfNotFoundInProductions &&
                        q.ProductionIds == expectedProductionIds)),
                Times.Once);
        }

        [Test]
        public async Task GetRowValuesBuilderAsync_ShouldReturnBuilder_WhenSingleImageExistsForEveryDocument()
        {
            // Arrange
            var imageFile1 = new ImageFile(1, "1", "Location1", "Name1", 0);
            var imageFile2 = new ImageFile(2, "2", "Location2", "Name2", 0);
            var imageFile3 = new ImageFile(3, "3", "Location3", "Name3", 0);

            var expectedDocumentToImageFiles = new Dictionary<int, ImageFile[]>()
            {
                { 1, new[] { imageFile1 } },
                { 2, new[] { imageFile2 } },
                { 3, new[] { imageFile3 } },
            };

            var imageFiles = new ImageFile[] { imageFile1, imageFile2, imageFile3 };

            SetupImageFileRepositoryForDocumentIds(imageFiles);

            // Act
            var result = await _sut.GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ID, _DOCUMENT_ARTIFACT_IDS).ConfigureAwait(false) as ImageInfoRowValuesBuilder;

            // Assert
            result.DocumentToImageFiles.Should().BeEquivalentTo(expectedDocumentToImageFiles);
        }

        [Test]
        public async Task GetRowValuesBuilderAsync_ShouldReturnBuilder_WhenMultipleImagesExistForSomeDocuments()
        {
            // Arrange
            var imageFile1a = new ImageFile(1, "1a", "Location1a", "Name1a", 0);
            var imageFile1b = new ImageFile(1, "1b", "Location1b", "Name1b", 0);
            var imageFile2 = new ImageFile(2, "2", "Location2", "Name2", 0);
            var imageFile3 = new ImageFile(3, "3", "Location3", "Name3", 0);

            var expectedDocumentToImageFiles = new Dictionary<int, ImageFile[]>()
            {
                { 1, new[] { imageFile1a, imageFile1b } },
                { 2, new[] { imageFile2 } },
                { 3, new[] { imageFile3 } },
            };

            var imageFiles = new ImageFile[] { imageFile1a, imageFile1b, imageFile2, imageFile3 };

            SetupImageFileRepositoryForDocumentIds(imageFiles);

            // Act
            var result = await _sut.GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ID, _DOCUMENT_ARTIFACT_IDS).ConfigureAwait(false) as ImageInfoRowValuesBuilder;

            // Assert
            result.DocumentToImageFiles.Should().BeEquivalentTo(expectedDocumentToImageFiles);
        }

        [Test]
        public async Task GetRowValuesBuilderAsync_ShouldReturnBuilder_WhenSomeDocumentsHaveNoImages()
        {
            // Arrange
            var imageFile1 = new ImageFile(1, "1", "Location1", "Name1", 0);
            var imageFile3 = new ImageFile(3, "3", "Location3", "Name3", 0);

            var expectedDocumentToImageFiles = new Dictionary<int, ImageFile[]>()
            {
                { 1, new[] { imageFile1 } },
                { 2, new ImageFile[] { } },
                { 3, new[] { imageFile3 } },
            };

            var imageFiles = new ImageFile[] { imageFile1, imageFile3 };

            SetupImageFileRepositoryForDocumentIds(imageFiles);

            // Act
            var result = await _sut.GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ID, _DOCUMENT_ARTIFACT_IDS).ConfigureAwait(false) as ImageInfoRowValuesBuilder;

            // Assert
            result.DocumentToImageFiles.Should().BeEquivalentTo(expectedDocumentToImageFiles);
        }

        [Test]
        public async Task GetRowValuesBuilderAsync_ShouldLogWarning_WhenFoundImagesForDocumentNotSelectedForSync()
        {
            // Arrange
            const int nonExistingDocumentId = 4;

            var imageFile1 = new ImageFile(1, "1", "Location1", "Name1", 0);
            var imageFile2 = new ImageFile(2, "2", "Location2", "Name2", 0);
            var imageFile3 = new ImageFile(3, "3", "Location3", "Name3", 0);
            var imageFileForNotExistingDocument = new ImageFile(nonExistingDocumentId, "4", "Location4", "Name4", 0);

            var imageFiles = new ImageFile[] { imageFile1, imageFile2, imageFile3, imageFileForNotExistingDocument };

            SetupImageFileRepositoryForDocumentIds(imageFiles);

            // Act
            var result = await _sut.GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ID, _DOCUMENT_ARTIFACT_IDS).ConfigureAwait(false) as ImageInfoRowValuesBuilder;

            // Assert
            _syncLogMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.Is<string>(s => s.Contains(nonExistingDocumentId.ToString()))), Times.Once);
        }

        private void SetupImageFileRepositoryForDocumentIds(IEnumerable<ImageFile> imageFiles)
        {
            _imageFileRepositoryMock.Setup(x => x.QueryImagesForDocumentsAsync(_SOURCE_WORKSPACE_ID, _DOCUMENT_ARTIFACT_IDS, It.IsAny<QueryImagesOptions>()))
                .ReturnsAsync(imageFiles);
        }
    }
}
