using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;
using Relativity.Sync.Transfer.ADF;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal class IsADFTransferEnabledTests
    {
        private IsADFTransferEnabled _sut;
        private Mock<IADLSMigrationStatus> _migrationStatusMock;
        private Mock<ISyncToggles> _syncTogglesMock;
        private Mock<IInstanceSettings> _instanceSettingsMock;
        private Mock<IDocumentSynchronizationConfiguration> _documentConfigurationMock;
        private Mock<IAPILog> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _migrationStatusMock = new Mock<IADLSMigrationStatus>();
            _syncTogglesMock = new Mock<ISyncToggles>();
            _instanceSettingsMock = new Mock<IInstanceSettings>();
            _documentConfigurationMock = new Mock<IDocumentSynchronizationConfiguration>();
            _loggerMock = new Mock<IAPILog>();
            _sut = new IsADFTransferEnabled(
                _migrationStatusMock.Object,
                _syncTogglesMock.Object,
                _instanceSettingsMock.Object,
                _documentConfigurationMock.Object,
                _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            typeof(IsADFTransferEnabled)
                .GetField("_isAdfTransferEnabled", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_sut, null);
        }

        [TestCase(false, false, ImportNativeFileCopyMode.DoNotImportNativeFiles, true)]
        [TestCase(true, false, ImportNativeFileCopyMode.DoNotImportNativeFiles, true)]
        [TestCase(false, true, ImportNativeFileCopyMode.DoNotImportNativeFiles, true)]
        [TestCase(true, true, ImportNativeFileCopyMode.CopyFiles, false)]
        public async Task ADFEnabler_ShouldUseADFTransferAsync_ShouldReturnTrue(bool useFMS, bool tenantIsMigrated, ImportNativeFileCopyMode importNativeFileCopyMode, bool forceADF)
        {
            // ARRANGE
            _syncTogglesMock.Setup(x => x.IsEnabled<UseFMS>()).Returns(useFMS);
            _migrationStatusMock.Setup(x => x.IsTenantFullyMigratedAsync()).ReturnsAsync(tenantIsMigrated);
            _instanceSettingsMock.Setup(x => x.GetShouldForceADFTransferAsync(It.IsAny<bool>())).ReturnsAsync(forceADF);
            _documentConfigurationMock.Setup(x => x.ImportNativeFileCopyMode).Returns(importNativeFileCopyMode);

            // ACT
            bool shouldUseADFToCopyFiles = _sut.Value;

            // ASSERT
            shouldUseADFToCopyFiles.Should().BeTrue();
        }

        [TestCase(false, false, ImportNativeFileCopyMode.DoNotImportNativeFiles, false)]
        [TestCase(true, false, ImportNativeFileCopyMode.CopyFiles, false)]
        [TestCase(false, true, ImportNativeFileCopyMode.CopyFiles, false)]
        [TestCase(true, true, ImportNativeFileCopyMode.DoNotImportNativeFiles, false)]
        public async Task ADFEnabler_ShouldUseADFTransferAsync_ShouldReturnFalse(bool useFMS, bool tenantIsMigrated, ImportNativeFileCopyMode importNativeFileCopyMode, bool forceADF)
        {
            // ARRANGE
            _syncTogglesMock.Setup(x => x.IsEnabled<UseFMS>()).Returns(useFMS);
            _migrationStatusMock.Setup(x => x.IsTenantFullyMigratedAsync()).ReturnsAsync(tenantIsMigrated);
            _instanceSettingsMock.Setup(x => x.GetShouldForceADFTransferAsync(It.IsAny<bool>())).ReturnsAsync(forceADF);
            _documentConfigurationMock.Setup(x => x.ImportNativeFileCopyMode).Returns(importNativeFileCopyMode);

            // ACT
            bool shouldUseADFToCopyFiles = _sut.Value;

            // ASSERT
            shouldUseADFToCopyFiles.Should().BeFalse();
        }
    }
}