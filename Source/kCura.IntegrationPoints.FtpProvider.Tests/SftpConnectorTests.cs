using System;
using FluentAssertions;
using kCura.IntegrationPoints.FtpProvider.Connection;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.FtpProvider.Tests
{
	[TestFixture]
	public class SftpConnectorTests
	{
		private Mock<IHostValidator> _hostValidatorMock;
		private Mock<IAPILog> _loggerMock;

		[SetUp]
		public void SetUp()
		{
			_hostValidatorMock = new Mock<IHostValidator>();
			_loggerMock = new Mock<IAPILog>();
		}

		[Test]
		public void Constructor_ShouldNotThrow()
		{
			// Arrange
			const string host = "1.2.3.4";
			const int port = 22;
			const string user = "user";
			const string password = "1234";

			Func<SftpConnector> createObjectAction = () => new SftpConnector(host, port, user, password, _hostValidatorMock.Object, _loggerMock.Object);

			// Act & Assert
			SftpConnector sftpConnector = createObjectAction();
			sftpConnector.Should().NotBeNull();
		}
	}
}