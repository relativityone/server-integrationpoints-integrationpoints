using FluentAssertions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.FtpProvider.Connection;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FtpProvider.Tests
{
    [TestFixture]
    public class HostValidatorTests
    {
        private Mock<IInstanceSettingsManager> _instanceSettingManagerFake;
        private Mock<IManagerFactory> _managerFactoryFake;
        private HostValidator _sut;
        private const string _HOST_IP = "55.66.77.88";

        [SetUp]
        public void SetUp()
        {
            _instanceSettingManagerFake = new Mock<IInstanceSettingsManager>();
            _managerFactoryFake = new Mock<IManagerFactory>();
            _managerFactoryFake.Setup(x => x.CreateInstanceSettingsManager())
                .Returns(_instanceSettingManagerFake.Object);
            _sut = new HostValidator(_managerFactoryFake.Object);
        }

        [Test]
        public void CanConnectTo_ShouldReturnTrueForLocalhost()
        {
            // act
            bool canConnect = _sut.CanConnectTo("localhost");

            // assert
            canConnect.Should().BeTrue();
        }

        [Test]
        public void CanConnectTo_ShouldReturnFalseForLocalhost()
        {
            // arrange
            const string localhost = "127.0.0.1";
            _instanceSettingManagerFake.Setup(x => x.RetrieveBlockedIPs()).Returns(localhost);

            // act
            bool canConnect = _sut.CanConnectTo(localhost);

            // assert
            canConnect.Should().BeFalse();
        }

        [TestCase(null, _HOST_IP, true)]
        [TestCase("", _HOST_IP, true)]
        [TestCase("127.0.0.1", _HOST_IP, true)]
        [TestCase("127.0.0.1;222.222.222.222", _HOST_IP, true)]
        [TestCase(_HOST_IP, _HOST_IP, false)]
        [TestCase(_HOST_IP + ";222.222.222.222", _HOST_IP, false)]
        [TestCase("222.222.222.222;" + _HOST_IP, _HOST_IP, false)]
        public void CanConnectTo_ShouldReturnExpectedCanConnectResult(string blockedIPs, string targetIP, bool expectedCanConnectResult)
        {
            // arrange
            _instanceSettingManagerFake.Setup(x => x.RetrieveBlockedIPs()).Returns(blockedIPs);

            // act
            bool canConnect = _sut.CanConnectTo(targetIP);

            // assert
            canConnect.Should().Be(expectedCanConnectResult);
        }
    }
}
