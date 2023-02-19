using FluentAssertions;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Web;

namespace kCura.IntegrationPoints.Web.Tests.Infrastructure.Session
{
    [TestFixture, Category("Unit")]
    public class SessionServiceFactoryTests
    {
        private Mock<IAPILog> _loggerMock;
        private Mock<Func<ICPHelper>> _helperFactoryMock;
        private Mock<Func<IAPILog>> _loggerFactoryMock;
        private const string _SESSION_KEY = "__WEB_SESSION_KEY__";

        [SetUp]
        public void SetUp()
        {
            _helperFactoryMock = new Mock<Func<ICPHelper>>();
            _helperFactoryMock
                .Setup(x => x())
                .Returns((ICPHelper)null);

            _loggerMock = new Mock<IAPILog>();
            _loggerMock
                .Setup(x => x.ForContext<SessionService>())
                .Returns(_loggerMock.Object);

            _loggerFactoryMock = new Mock<Func<IAPILog>>();
            _loggerFactoryMock
                .Setup(x => x())
                .Returns(_loggerMock.Object);
        }

        [Test]
        public void ShouldReuseExistingSession()
        {
            // arrange
            var sessionServiceInSessionState = new SessionService(
                connectionHelper: null,
                logger: _loggerMock.Object
            );

            var httpSessionStateMock = new Mock<HttpSessionStateBase>();
            httpSessionStateMock.Setup(x => x[_SESSION_KEY]).Returns(sessionServiceInSessionState);
            HttpContextBase httpContextMock = GetHttpContextMockWithSession(httpSessionStateMock.Object);

            // act
            ISessionService actualSessionService = SessionServiceFactory.GetSessionService(
                _helperFactoryMock.Object,
                _loggerFactoryMock.Object,
                httpContextMock
            );

            // assert
            actualSessionService.Should()
                .Be(sessionServiceInSessionState, "because session service was present in http session state");
            _helperFactoryMock.Verify(x => x(), Times.Never, "because provided factory should not be used when session already exists");
            _loggerFactoryMock.Verify(x => x(), Times.Never, "because logger factory should not be used when session already exists");
        }

        [Test]
        public void ShouldCreateNewSessionWhenSessionStateIsNull()
        {
            // arrange
            HttpContextBase httpContextMock = GetHttpContextMockWithSession(httpSessionState: null);

            // act
            ISessionService actualSessionService = SessionServiceFactory.GetSessionService(
                _helperFactoryMock.Object,
                _loggerFactoryMock.Object,
                httpContextMock
            );

            // assert
            actualSessionService.Should().NotBeNull("because new session service should be created");
            _helperFactoryMock.Verify(x => x(), Times.Once, "because provided factory should be used to create helper");
            _loggerFactoryMock.Verify(x => x(), Times.Once, "because logger factory should be used to create helper");
        }

        [Test]
        public void ShouldCreateNewSessionWhenSessionIsNotPresentInSessionState()
        {
            // arrange
            SessionService sessionServiceInSessionState = null;
            var httpSessionStateMock = new Mock<HttpSessionStateBase>();
            httpSessionStateMock.Setup(x => x[_SESSION_KEY]).Returns(sessionServiceInSessionState);
            HttpContextBase httpContextMock = GetHttpContextMockWithSession(httpSessionStateMock.Object);

            // act
            ISessionService actualSessionService = SessionServiceFactory.GetSessionService(
                _helperFactoryMock.Object,
                _loggerFactoryMock.Object,
                httpContextMock
            );

            // assert
            actualSessionService.Should().NotBeNull("because new session service should be created");
            _helperFactoryMock.Verify(x => x(), Times.Once, "because provided factory should be used to create helper");
            _loggerFactoryMock.Verify(x => x(), Times.Once, "because logger factory should be used to create helper");
        }

        [Test]
        public void ShouldAddNewSessionToSessionStateWhenSessionIsNotPresentInSessionState()
        {
            // arrange
            SessionService sessionServiceInSessionState = null;
            var httpSessionStateMock = new Mock<HttpSessionStateBase>();
            httpSessionStateMock.Setup(x => x[_SESSION_KEY]).Returns(sessionServiceInSessionState);
            HttpContextBase httpContextMock = GetHttpContextMockWithSession(httpSessionStateMock.Object);

            // act
            ISessionService actualSessionService = SessionServiceFactory.GetSessionService(
                _helperFactoryMock.Object,
                _loggerFactoryMock.Object,
                httpContextMock
            );

            // assert
            httpSessionStateMock.VerifySet(x => x[_SESSION_KEY] = It.IsAny<ISessionService>());
        }

        [Test]
        public void ShouldReuseSessionServiceInSecondRequestWhenSessionStateIsNotNull()
        {
            // arrange
            object sessionServiceInSessionState = null;
            var httpSessionStateMock = new Mock<HttpSessionStateBase>();
            httpSessionStateMock.SetupGet(x => x[_SESSION_KEY]).Returns(() => sessionServiceInSessionState);
            httpSessionStateMock.SetupSet(x => x[_SESSION_KEY] = It.IsAny<ISessionService>())
                .Callback<string, object>((key, value) => sessionServiceInSessionState = value);
            HttpContextBase httpContextMock = GetHttpContextMockWithSession(httpSessionStateMock.Object);

            // act
            ISessionService firstSessionService = SessionServiceFactory.GetSessionService(
                _helperFactoryMock.Object,
                _loggerFactoryMock.Object,
                httpContextMock
            );
            ISessionService secondSessionService = SessionServiceFactory.GetSessionService(
                _helperFactoryMock.Object,
                _loggerFactoryMock.Object,
                httpContextMock
            );

            // assert
            firstSessionService.Should().NotBeNull();
            secondSessionService.Should().Be(firstSessionService);
        }

        [Test]
        public void ShouldNotReuseSessionServiceInSecondRequestWhenSessionStateIsNull()
        {
            // arrange
            HttpContextBase httpContextMock = GetHttpContextMockWithSession(httpSessionState: null);

            // act
            ISessionService firstSessionService = SessionServiceFactory.GetSessionService(
                _helperFactoryMock.Object,
                _loggerFactoryMock.Object,
                httpContextMock
            );
            ISessionService secondSessionService = SessionServiceFactory.GetSessionService(
                _helperFactoryMock.Object,
                _loggerFactoryMock.Object,
                httpContextMock
            );

            // assert
            firstSessionService.Should().NotBeNull();
            secondSessionService.Should().NotBeNull();
            secondSessionService.Should().NotBe(firstSessionService);
        }

        private HttpContextBase GetHttpContextMockWithSession(HttpSessionStateBase httpSessionState)
        {
            var mock = new Mock<HttpContextBase>();
            mock.Setup(x => x.Session).Returns(httpSessionState);
            return mock.Object;
        }
    }
}
