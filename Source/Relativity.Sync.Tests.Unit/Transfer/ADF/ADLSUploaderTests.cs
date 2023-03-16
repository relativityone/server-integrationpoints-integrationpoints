using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Storage;
using Relativity.Sync.Transfer.ADLS;

namespace Relativity.Sync.Tests.Unit.Transfer.ADF
{
    [TestFixture]
    internal class ADLSUploaderTests
    {
        private const string _SOURCE_FILE_PATH = @"\\Adler\Sieben";
        private const string _EXCEPTION_MESSAGE = "Some Exception";

        private Mock<IStorageAccessService> _storageAccessServiceMock;
        private Mock<IAPILog> _loggerFake;
        private AdlsUploader _sut;
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
                "Temp",
                "RIP_BatchFiles");

            _storageAccessServiceMock = new Mock<IStorageAccessService>();

            _loggerFake = new Mock<IAPILog>();
            _loggerFake.Setup(x => x.ForContext<AdlsUploader>()).Returns(_loggerFake.Object);

            _sut = new AdlsUploader(_storageAccessServiceMock.Object, _loggerFake.Object);

            typeof(AdlsUploader)
                .GetField("_betweenRetriesBase", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_sut, 0.1);
        }

        [Test]
        public async Task ADLSUploader_UploadFileAsync_ShouldReturnValidDestinationFilePath()
        {
            // Arrange
            _storageAccessServiceMock.Setup(x => x.GetStorageEndpointsAsync()).ReturnsAsync(_storageEndpoints);

            // Act
            string destinationFilePath = await _sut.UploadFileAsync(_SOURCE_FILE_PATH, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Path.GetDirectoryName(destinationFilePath).Should().Be(_destinationDir);
            Path.GetExtension(destinationFilePath).Should().BeEquivalentTo(".csv");
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

            _storageAccessServiceMock.Setup(x => x.CopyFileAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<CopyFileOptions>(y => y.DestinationParentDirectoryNotExistsBehavior == copyFileOptions.DestinationParentDirectoryNotExistsBehavior &&
                                            y.DestinationExistsBehavior == copyFileOptions.DestinationExistsBehavior),
                It.IsAny<CancellationToken>())).Throws(new Exception(_EXCEPTION_MESSAGE));

            _storageAccessServiceMock.Setup(x => x.GetStorageEndpointsAsync()).ReturnsAsync(_storageEndpoints);

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
            _storageAccessServiceMock.Setup(x => x.GetStorageEndpointsAsync()).ReturnsAsync(_storageEndpoints);

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
            _storageAccessServiceMock.Setup(x => x.GetStorageEndpointsAsync()).ReturnsAsync(_storageEndpoints);

            // Act
            token.Cancel();
            Func<Task> act = async () => { await _sut.UploadFileAsync(_SOURCE_FILE_PATH, token.Token).ConfigureAwait(false); };

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("The operation was canceled.");
            _loggerFake.Verify(x => x.LogWarning("ADLS Batch file upload cancelled."), Times.Once);
            _loggerFake.Verify(x => x.LogWarning(It.IsAny<Exception>(), It.Is<string>(y => y.Contains("Encountered issue while loading file to ADLS, attempting to retry.")), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }
    }
}
