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
        private Mock<IAPILog> _loggerFake;

        private const string _HOST = "1.2.3.4";
        private const int _PORT = 22;
        private const string _USER = "User";
        private const string _PASSWORD = "1234";

        [SetUp]
        public void SetUp()
        {
            _hostValidatorMock = new Mock<IHostValidator>();
            _loggerFake = new Mock<IAPILog>();
        }

        [Test]
        public void Constructor_ShouldNotThrow()
        {
            // Arrange
            Func<SftpConnector> createObjectAction = () => new SftpConnector(_HOST, _PORT, _USER, _PASSWORD, _hostValidatorMock.Object, _loggerFake.Object);

            // Act & Assert
            SftpConnector sftpConnector = createObjectAction();
            sftpConnector.Should().NotBeNull();
        }

        [Test]
        public void TestConnection_ShouldValidateHost()
        {
            // Arrange
            SftpConnector sut = new SftpConnector(_HOST, _PORT, _USER, _PASSWORD, _hostValidatorMock.Object, _loggerFake.Object);

            // Act
            bool result = sut.TestConnection();

            // Assert
            result.Should().BeFalse();
            _hostValidatorMock.Verify(x => x.CanConnectTo(_HOST));
        }
    }
}