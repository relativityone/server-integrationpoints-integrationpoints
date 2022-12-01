using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;
using Relativity.Sync.Transfer.ADLS;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal class IsAdfTransferEnabledTests
    {
        private IsAdfTransferEnabled _sut;
        private Mock<IAdlsMigrationStatus> _migrationStatusMock;
        private Mock<ISyncToggles> _syncTogglesMock;
        private Mock<IInstanceSettings> _instanceSettingsMock;
        private Mock<IDocumentSynchronizationConfiguration> _documentConfigurationMock;
        private Mock<IAPILog> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _migrationStatusMock = new Mock<IAdlsMigrationStatus>();
            _syncTogglesMock = new Mock<ISyncToggles>();
            _instanceSettingsMock = new Mock<IInstanceSettings>();
            _documentConfigurationMock = new Mock<IDocumentSynchronizationConfiguration>();
            _loggerMock = new Mock<IAPILog>();
            _sut = new IsAdfTransferEnabled(
                _migrationStatusMock.Object,
                _syncTogglesMock.Object,
                _instanceSettingsMock.Object,
                _documentConfigurationMock.Object,
                _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            typeof(IsAdfTransferEnabled)
                .GetField("_isAdfTransferEnabled", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_sut, null);
        }

        [TestCase(false, false, ImportNativeFileCopyMode.CopyFiles, true)]
        [TestCase(true, false, ImportNativeFileCopyMode.CopyFiles, true)]
        [TestCase(false, true, ImportNativeFileCopyMode.CopyFiles, true)]
        [TestCase(true, true, ImportNativeFileCopyMode.CopyFiles, false)]
        public void ADFEnabler_ShouldUseADFTransfer_WhenConditionsAreMet(bool useFMS, bool tenantIsMigrated, ImportNativeFileCopyMode importNativeFileCopyMode, bool forceADF)
        {
            // Arrange
            _syncTogglesMock.Setup(x => x.IsEnabled<UseFmsToggle>()).Returns(useFMS);
            _migrationStatusMock.Setup(x => x.IsTenantFullyMigratedAsync()).ReturnsAsync(tenantIsMigrated);
            _instanceSettingsMock.Setup(x => x.GetShouldForceADFTransferAsync(It.IsAny<bool>())).ReturnsAsync(forceADF);
            _documentConfigurationMock.Setup(x => x.ImportNativeFileCopyMode).Returns(importNativeFileCopyMode);

            // Act
            bool shouldUseADFToCopyFiles = _sut.Value;

            // Assert
            shouldUseADFToCopyFiles.Should().BeTrue();
        }

        [TestCase(false, false, ImportNativeFileCopyMode.DoNotImportNativeFiles, false)]
        [TestCase(true, false, ImportNativeFileCopyMode.CopyFiles, false)]
        [TestCase(false, true, ImportNativeFileCopyMode.CopyFiles, false)]
        [TestCase(true, true, ImportNativeFileCopyMode.DoNotImportNativeFiles, false)]
        public void ADFEnabler_ShouldNotUseADFTransfer_WhenConditionsAreNotMet(bool useFMS, bool tenantIsMigrated, ImportNativeFileCopyMode importNativeFileCopyMode, bool forceADF)
        {
            // Arrange
            _syncTogglesMock.Setup(x => x.IsEnabled<UseFmsToggle>()).Returns(useFMS);
            _migrationStatusMock.Setup(x => x.IsTenantFullyMigratedAsync()).ReturnsAsync(tenantIsMigrated);
            _instanceSettingsMock.Setup(x => x.GetShouldForceADFTransferAsync(It.IsAny<bool>())).ReturnsAsync(forceADF);
            _documentConfigurationMock.Setup(x => x.ImportNativeFileCopyMode).Returns(importNativeFileCopyMode);

            // Act
            bool shouldUseADFToCopyFiles = _sut.Value;

            // Assert
            shouldUseADFToCopyFiles.Should().BeFalse();
        }

        [TestCase(ImportNativeFileCopyMode.DoNotImportNativeFiles)]
        [TestCase(ImportNativeFileCopyMode.SetFileLinks)]
        public void ADFEnable_ShouldNotUseADFTransfer_WhenNativeFileCopyModeIsNotCopyFiles(ImportNativeFileCopyMode importNativeFileCopyMode)
        {
            // Arrange
            _documentConfigurationMock.Setup(x => x.ImportNativeFileCopyMode).Returns(importNativeFileCopyMode);

            // Act
            bool shouldUseADFToCopyFiles = _sut.Value;

            // Assert
            shouldUseADFToCopyFiles.Should().BeFalse();

            _syncTogglesMock.Verify(x => x.IsEnabled<UseFmsToggle>(), Times.Never);
            _migrationStatusMock.Verify(x => x.IsTenantFullyMigratedAsync(), Times.Never);
            _instanceSettingsMock.Verify(x => x.GetShouldForceADFTransferAsync(It.IsAny<bool>()), Times.Never);
        }
    }
}
