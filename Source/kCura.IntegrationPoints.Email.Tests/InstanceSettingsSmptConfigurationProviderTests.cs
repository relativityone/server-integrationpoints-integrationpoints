using FluentAssertions;
using kCura.IntegrationPoints.Email.Dto;
using LanguageExt;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;

namespace kCura.IntegrationPoints.Email.Tests
{
    [TestFixture, Category("Unit")]
    internal class InstanceSettingsSmptConfigurationProviderTests
    {
        private InstanceSettingsSmptConfigurationProvider _sut;
        private Mock<IInstanceSettingsBundle> _instanceSettingsBundleMock;
        private Mock<IAPILog> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _instanceSettingsBundleMock = new Mock<IInstanceSettingsBundle>();
            _loggerMock = new Mock<IAPILog>();
            _loggerMock
                .Setup(x => x.ForContext<InstanceSettingsSmptConfigurationProvider>())
                .Returns(_loggerMock.Object);

            _sut = new InstanceSettingsSmptConfigurationProvider(_loggerMock.Object, _instanceSettingsBundleMock.Object);
        }

        [Test]
        public void ShouldReturnSomeWhenInstanceSettingsProviderNotThrowingExceptions()
        {
            // act
            Option<SmtpConfigurationDto> smtpConfigurationOption = _sut.GetConfiguration();

            // assert
            smtpConfigurationOption.IsSome.Should().BeTrue($"because {nameof(IInstanceSettingsBundle)} has not throw any exception");
        }

        [Test]
        public void ShouldReturnNoneWhenInstanceSettingsProviderThrewException()
        {
            // arrange
            _instanceSettingsBundleMock
                .Setup(x => x.GetBool(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            // act
            Option<SmtpConfigurationDto> smtpConfigurationOption = _sut.GetConfiguration();

            // assert
            smtpConfigurationOption.IsNone.Should().BeTrue($"because {nameof(IInstanceSettingsBundle)} threw an exception");
        }

        [Test]
        public void ShouldLogErrorWhenInstanceSettingsProviderThrewException()
        {
            // arrange
            Exception exceptionToThrow = new Exception();
            _instanceSettingsBundleMock
                .Setup(x => x.GetBool(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(exceptionToThrow);

            // act
            _sut.GetConfiguration();

            // assert
            _loggerMock.Verify(x =>
                x.LogError(exceptionToThrow, "Failed to read SMTP configuration from instance settings.")
            );
        }
    }
}
