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

		[Test]
		public void CanConnectTo_ShouldReturnTrueForIP()
		{
			// act
			bool canConnect = _sut.CanConnectTo("55.66.77.88");

			// assert
			canConnect.Should().BeTrue();
		}

		[Test]
		public void CanConnectTo_ShouldReturnFalseForIP()
		{
			// arrange
			const string ip = "55.66.77.88";
			_instanceSettingManagerFake.Setup(x => x.RetrieveBlockedIPs()).Returns(ip);

			// act
			bool canConnect = _sut.CanConnectTo(ip);

			// assert
			canConnect.Should().BeFalse();
		}
	}
}