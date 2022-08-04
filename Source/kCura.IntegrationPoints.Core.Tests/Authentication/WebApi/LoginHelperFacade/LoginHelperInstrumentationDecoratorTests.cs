using System;
using System.Net;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Authentication.WebApi.LoginHelperFacade
{
    [TestFixture, Category("Unit")]
    public class LoginHelperInstrumentationDecoratorTests
    {
        private LoginHelperInstrumentationDecorator _sut;
        private Mock<ILoginHelperFacade> _loginHelperFacadeMock;
        private Mock<IExternalServiceInstrumentationProvider> _externalServiceInstrumentationProviderMock;
        private Mock<IExternalServiceSimpleInstrumentation> _externalServiceSimpleInstrumentationMock;

        [SetUp]
        public void SetUp()
        {
            _loginHelperFacadeMock = new Mock<ILoginHelperFacade>();
            _externalServiceSimpleInstrumentationMock = new Mock<IExternalServiceSimpleInstrumentation>();
            _externalServiceSimpleInstrumentationMock
                .Setup(x => x.Execute(It.IsAny<Func<NetworkCredential>>()))
                .Callback<Func<NetworkCredential>>(instrumentedCall => instrumentedCall());
            _externalServiceInstrumentationProviderMock = new Mock<IExternalServiceInstrumentationProvider>();
            _externalServiceInstrumentationProviderMock
                .Setup(x => x.CreateSimple(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>())
                )
                .Returns(_externalServiceSimpleInstrumentationMock.Object);

            _sut = new LoginHelperInstrumentationDecorator(
                _loginHelperFacadeMock.Object,
                _externalServiceInstrumentationProviderMock.Object);
        }

        [Test]
        public void ShouldWrapCallInInstrumentation()
        {
            // act
            string token = "testToken";
            var cookieContainer = new CookieContainer();

            _sut.LoginUsingAuthToken(token, cookieContainer);

            // assert
            _externalServiceSimpleInstrumentationMock.Verify(x => x.Execute(It.IsAny<Func<NetworkCredential>>()));
            _loginHelperFacadeMock.Verify(x => x.LoginUsingAuthToken(token, cookieContainer));
        }
    }
}
