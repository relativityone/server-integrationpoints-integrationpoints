using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Storage;
using Relativity.Sync.Transfer.ADF;

namespace Relativity.Sync.Tests.Unit.Transfer.ADF
{
    [TestFixture]
    internal class ADLSUploaderTests
    {
        private const string _SOURCE_FILE_PATH = @"\\Adler\Sieben";
        private const string _EXCEPTION_MESSAGE = "Some Exception";

        private Mock<IHelperWrapper> _helperMock;
        private Mock<IStorageAccess<string>> _storageAccessMock;
        private Mock<IAPILog> _loggerFake;
        private ADLSUploader _sut;
        private StorageEndpoint[] _storageEndpoints;
        private string _destinationDir;

        [SetUp]
        public void Setup()
        {
            _storageEndpoints = new[]
            {
                new StorageEndpoint
                {
                    EndpointFqdn = "AzureADLSFQDNEndpoint",
                    PrimaryStorageContainer = "T-Adler7"
                }
            };

            _destinationDir = Path.Combine(
                @"\\",
                _storageEndpoints[0].EndpointFqdn,
                _storageEndpoints[0].PrimaryStorageContainer,
                "Files",
                "RIP_BatchFiles");

            _helperMock = new Mock<IHelperWrapper>();

            _loggerFake = new Mock<IAPILog>();
            _loggerFake.Setup(x => x.ForContext<ADLSUploader>()).Returns(_loggerFake.Object);

            _storageAccessMock = new Mock<IStorageAccess<string>>();
            _helperMock.Setup(x => x.GetStorageAccessorAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_storageAccessMock.Object);

            _sut = new ADLSUploader(_helperMock.Object, _loggerFake.Object);
        }

        [Test]
        public async Task ADLSUploader_UploadFileAsync_ShouldReturnValidDestinationFilePath()
        {
            // Arrange
            _helperMock.Setup(x => x.GetStorageEndpointsAsync(CancellationToken.None)).ReturnsAsync(_storageEndpoints);

            // Act
            string destinationFilePath = await _sut.UploadFileAsync(_SOURCE_FILE_PATH, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Path.GetDirectoryName(destinationFilePath).Should().Be(_destinationDir);
            Path.GetExtension(destinationFilePath).Should().BeEquivalentTo(".csv");
        }

        [Test]
        public void ADLSUploader_UploadFileAsync_ShouldThrowErrorWhenCreateDirectoryAsyncFails()
        {
            // Arrange
            _storageAccessMock.Setup(x => x.CreateDirectoryAsync(
                It.IsAny<string>(),
                It.IsAny<CreateDirectoryOptions>(),
                It.IsAny<CancellationToken>())).Throws(new Exception(_EXCEPTION_MESSAGE));

            _helperMock.Setup(x => x.GetStorageEndpointsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_storageEndpoints);

            // Act
            Func<Task<string>> function = async () => await _sut.UploadFileAsync(_SOURCE_FILE_PATH, CancellationToken.None).ConfigureAwait(false);

            // Assert
            function.Should().Throw<Exception>().WithMessage(_EXCEPTION_MESSAGE);
            _loggerFake.Verify(x => x.LogError(_EXCEPTION_MESSAGE), Times.Once);
            _loggerFake.Verify(x => x.LogWarning(It.IsAny<Exception>(), It.Is<string>(y => y.Contains("Encountered issue while loading file to ADLS, attempting to retry.")), It.IsAny<int>(), It.IsAny<double>()), Times.Exactly(3));
        }

        [Test]
        public void ADLSUploader_UploadFileAsync_ShouldThrowErrorWhenCopyFileAsyncFails()
        {
            // Arrange
            CopyFileOptions copyFileOptions = new CopyFileOptions
            {
                DestinationParentDirectoryNotExistsBehavior = DirectoryNotExistsBehavior.CreateIfNotExists,
                DestinationExistsBehavior = FileExistsBehavior.OverwriteIfExists
            };

            _storageAccessMock.Setup(x => x.CopyFileAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<CopyFileOptions>(y => y.DestinationParentDirectoryNotExistsBehavior == copyFileOptions.DestinationParentDirectoryNotExistsBehavior &&
                                            y.DestinationExistsBehavior == copyFileOptions.DestinationExistsBehavior),
                It.IsAny<CancellationToken>())).Throws(new Exception(_EXCEPTION_MESSAGE));

            _helperMock.Setup(x => x.GetStorageEndpointsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_storageEndpoints);

            // Act
            Func<Task<string>> function = async () => await _sut.UploadFileAsync(_SOURCE_FILE_PATH, CancellationToken.None).ConfigureAwait(false);

            // Assert
            function.Should().Throw<Exception>().WithMessage(_EXCEPTION_MESSAGE);
            _loggerFake.Verify(x => x.LogError(_EXCEPTION_MESSAGE), Times.Once);
            _loggerFake.Verify(x => x.LogWarning(It.IsAny<Exception>(), It.Is<string>(y => y.Contains("Encountered issue while loading file to ADLS, attempting to retry.")), It.IsAny<int>(), It.IsAny<double>()), Times.Exactly(3));
        }

        [Test]
        public void ADLSUploader_UploadFileAsync_ShouldReturnEmptyStringWhenSourceFilePathIsEmpty()
        {
            // Arrange
            _helperMock.Setup(x => x.GetStorageEndpointsAsync(CancellationToken.None)).ReturnsAsync(_storageEndpoints);

            // Act
            Func<Task<string>> function = async () => await _sut.UploadFileAsync(string.Empty, CancellationToken.None).ConfigureAwait(false);

            // Assert
            function.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ADLSUploader_UploadFileAsync_ShouldReturnEmptyStringWhenCancellationIsRequested()
        {
            // Arrange
            CancellationTokenSource token = new CancellationTokenSource();
            _helperMock.Setup(x => x.GetStorageEndpointsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_storageEndpoints);

            // Act
            token.Cancel();
            string destinationFilePath = await _sut.UploadFileAsync(_SOURCE_FILE_PATH, token.Token).ConfigureAwait(false);

            // Assert
            destinationFilePath.Should().BeEmpty();
            _loggerFake.Verify(x => x.LogWarning("ADLS Batch file upload cancelled."), Times.Once);
            _loggerFake.Verify(x => x.LogWarning(It.IsAny<Exception>(), It.Is<string>(y => y.Contains("Encountered issue while loading file to ADLS, attempting to retry.")), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }
    }
}
