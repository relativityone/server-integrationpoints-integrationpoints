using System;
using System.Net;
using FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Authentication.WebApi.LoginHelperFacade
{
    [TestFixture, Category("Unit")]
    public class LoginHelperRetryDecoratorTests
    {
        private LoginHelperRetryDecorator _sut;
        private Mock<ILoginHelperFacade> _loginHelperMock;

        [SetUp]
        public void SetUp()
        {
            var retryHandler = new RetryHandler(null, 1, 0);
            var retryHandlerFactory = new Mock<IRetryHandlerFactory>();
            retryHandlerFactory
                .Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<ushort>()))
                .Returns(retryHandler);

            _loginHelperMock = new Mock<ILoginHelperFacade>();

            _sut = new LoginHelperRetryDecorator(
                _loginHelperMock.Object,
                retryHandlerFactory.Object
            );
        }

        [Test]
        public void ShouldReturnResultAndRetryOnFailures()
        {
            // arrange
            var expectedResult = new NetworkCredential();

            _loginHelperMock
                .SetupSequence(x => x.LoginUsingAuthToken(
                    It.IsAny<string>(),
                    It.IsAny<CookieContainer>())
                )
                .Throws<InvalidOperationException>()
                .Returns(expectedResult);

            // act
            NetworkCredential result = _sut.LoginUsingAuthToken(It.IsAny<string>(), It.IsAny<CookieContainer>());

            // assert
            result.Should().Be(expectedResult);
        }
    }
}
