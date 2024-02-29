using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal class FileBaseTests
    {
        private Mock<IAntiMalwareHandler> _antiMalwareHandlerMock;

        [SetUp]
        public void SetUp()
        {
            _antiMalwareHandlerMock = new Mock<IAntiMalwareHandler>();
        }

        [Test]
        public async Task ValidateMalwareAsync_ShouldNotCheckingForMalware_WhenLocationIsEmpty()
        {
            // Arrange
            FileBase sut = new NativeFile(It.IsAny<int>(), string.Empty, It.IsAny<string>(), It.IsAny<long>());

            // Act
            await sut.ValidateMalwareAsync(_antiMalwareHandlerMock.Object).ConfigureAwait(false);

            // Assert
            _antiMalwareHandlerMock.Verify(x => x.ContainsMalwareAsync(It.IsAny<IFile>()), Times.Never());
        }

        [Test]
        public async Task ValidateMalwareAsync_ShouldNotCheckingForMalwareTwice_ForTheSameFile()
        {
            // Arrange
            const string location = "TestLocation";

            FileBase sut = new NativeFile(It.IsAny<int>(), location, It.IsAny<string>(), It.IsAny<long>());

            // Act
            await sut.ValidateMalwareAsync(_antiMalwareHandlerMock.Object).ConfigureAwait(false);

            await sut.ValidateMalwareAsync(_antiMalwareHandlerMock.Object).ConfigureAwait(false);

            // Assert
            _antiMalwareHandlerMock.Verify(x => x.ContainsMalwareAsync(It.IsAny<IFile>()), Times.Once);
        }
    }
}
