using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;

namespace Relativity.Sync.Tests.Unit.Pipelines
{
    [TestFixture]
    public class IAPIv2RunCheckerTests
    {
        private IIAPIv2RunChecker _sut;
        private Mock<ISyncToggles> _togglesMock;
        private Mock<IIAPIv2RunCheckerConfiguration> _runCheckerConfig;

        [SetUp]
        public void SetUp()
        {
            _togglesMock = new Mock<ISyncToggles>();
            _runCheckerConfig = new Mock<IIAPIv2RunCheckerConfiguration>();

            SetUpInitialValuesForPositiveCheck();

            _sut = new IAPIv2RunChecker(_runCheckerConfig.Object, _togglesMock.Object);
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnTrue_IfAllConditionsForGoldFlowAreMet()
        {
            // Act
            bool? result = _sut.ShouldBeUsed();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldCheckConditionsOnlyOnceAndThenStoreIt()
        {
            // Arrange
            _sut.ShouldBeUsed();

            // Act
            bool? result = _sut.ShouldBeUsed();

            // Assert
            result.Should().NotBeNull();
            _togglesMock.Verify(x => x.IsEnabled<EnableIAPIv2Toggle>(), Times.Once);
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfIAPIToggleIsDisabled()
        {
            // Arrange
            _togglesMock.Setup(x => x.IsEnabled<EnableIAPIv2Toggle>()).Returns(false);

            // Act
            bool? result = _sut.ShouldBeUsed();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeTrue();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfTransferredObjectIsNotDocument()
        {
            // Arrange
            _runCheckerConfig.SetupGet(x => x.RdoArtifactTypeId).Returns((int)ArtifactType.Client);

            // Act
            bool? result = _sut.ShouldBeUsed();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeTrue();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfJobIsRetried()
        {
            // Arrange
            _runCheckerConfig.SetupGet(x => x.IsRetried).Returns(true);

            // Act
            bool? result = _sut.ShouldBeUsed();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeTrue();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfJobIsDrainStopped()
        {
            // Arrange
            _runCheckerConfig.SetupGet(x => x.IsDrainStopped).Returns(true);

            // Act
            bool? result = _sut.ShouldBeUsed();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeTrue();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfJobHasLongTextFieldsMapped()
        {
            // Arrange
            _runCheckerConfig.SetupGet(x => x.HasLongTextFields).Returns(true);

            // Act
            bool? result = _sut.ShouldBeUsed();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeTrue();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfJobHasModeSetToCopyFiles()
        {
            // Arrange
            _runCheckerConfig.SetupGet(x => x.NativeBehavior).Returns(ImportNativeFileCopyMode.CopyFiles);

            // Act
            bool? result = _sut.ShouldBeUsed();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeTrue();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfImagesAreImported()
        {
            // Arrange
            _runCheckerConfig.SetupGet(x => x.ImageImport).Returns(true);

            // Act
            bool? result = _sut.ShouldBeUsed();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeTrue();
        }

        private void SetUpInitialValuesForPositiveCheck()
        {
            _togglesMock.Setup(x => x.IsEnabled<EnableIAPIv2Toggle>()).Returns(true);
            _runCheckerConfig.SetupGet(x => x.HasLongTextFields).Returns(false);
            _runCheckerConfig.SetupGet(x => x.ImageImport).Returns(false);
            _runCheckerConfig.SetupGet(x => x.IsDrainStopped).Returns(false);
            _runCheckerConfig.SetupGet(x => x.IsRetried).Returns(false);
            _runCheckerConfig.SetupGet(x => x.NativeBehavior).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);
            _runCheckerConfig.SetupGet(x => x.RdoArtifactTypeId).Returns((int)ArtifactType.Document);
        }
    }
}
