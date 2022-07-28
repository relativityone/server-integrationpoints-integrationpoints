using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;
using Relativity.Sync.Transfer.ADF;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal class ADFTransferEnablerTests
    {
        private ADFTransferEnabler _sut;
        private Mock<IADLSMigrationStatus> _migrationStatusMock;
        private Mock<ISyncToggles> _syncTogglesMock;
        private Mock<IInstanceSettings> _instanceSettingsMock;
        private Mock<IAPILog> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _migrationStatusMock = new Mock<IADLSMigrationStatus>();
            _syncTogglesMock = new Mock<ISyncToggles>();
            _instanceSettingsMock = new Mock<IInstanceSettings>();
            _loggerMock = new Mock<IAPILog>();
            _sut = new ADFTransferEnabler(_migrationStatusMock.Object, _syncTogglesMock.Object, _instanceSettingsMock.Object, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            typeof(ADFTransferEnabler).GetField("_isAdfTransferEnabled", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_sut, null);
        }

        [TestCase(false, false, true)]
        [TestCase(true, false, true)]
        [TestCase(false, true, true)]
        [TestCase(true, true, false)]
        public async Task ADFEnabler_ShouldUseADFTransferAsync_ShouldReturnTrue(bool useFMS, bool tenantIsMigrated, bool forceADF)
        {
            // ARRANGE
            _syncTogglesMock.Setup(x => x.IsEnabled<UseFMS>()).Returns(useFMS);
            _migrationStatusMock.Setup(x => x.IsTenantFullyMigratedAsync()).ReturnsAsync(tenantIsMigrated);
            _instanceSettingsMock.Setup(x => x.GetShouldForceADFTransferAsync(It.IsAny<bool>())).ReturnsAsync(forceADF);

            // ACT
            bool shouldUseADFToCopyFiles = _sut.IsAdfTransferEnabled;

            // ASSERT
            shouldUseADFToCopyFiles.Should().BeTrue();
        }

        [TestCase(false, false, false)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        public async Task ADFEnabler_ShouldUseADFTransferAsync_ShouldReturnFalse(bool useFMS, bool tenantIsMigrated, bool forceADF)
        {
            // ARRANGE
            _syncTogglesMock.Setup(x => x.IsEnabled<UseFMS>()).Returns(useFMS);
            _migrationStatusMock.Setup(x => x.IsTenantFullyMigratedAsync()).ReturnsAsync(tenantIsMigrated);
            _instanceSettingsMock.Setup(x => x.GetShouldForceADFTransferAsync(It.IsAny<bool>())).ReturnsAsync(forceADF);

            // ACT
            bool shouldUseADFToCopyFiles = _sut.IsAdfTransferEnabled;

            // ASSERT
            shouldUseADFToCopyFiles.Should().BeFalse();
        }
    }
}