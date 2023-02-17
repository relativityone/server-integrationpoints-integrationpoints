using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.UserContext;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Context.UserContext
{
    [TestFixture, Category("Unit")]
    public class SessionUserContextServiceTests
    {
        private Mock<ISessionService> _sessionServiceMock;
        private Mock<IUserContext> _nextUserContextServiceMock;
        private SessionUserContextService _sut;

        [SetUp]
        public void SetUp()
        {
            _sessionServiceMock = new Mock<ISessionService>();
            _nextUserContextServiceMock = new Mock<IUserContext>();

            _sut = new SessionUserContextService(
                _sessionServiceMock.Object,
                _nextUserContextServiceMock.Object
            );
        }

        [Test]
        public void GetUserID_ShouldReturnValueIfInjectedSessionServiceReturnsNonNull()
        {
            // arrange
            const int userID = 1019723;
            _sessionServiceMock
                .Setup(x => x.UserID)
                .Returns(userID);

            // act
            int result = _sut.GetUserID();

            // assert
            result.Should().Be(userID);
        }

        [Test]
        public void GetUserID_ShouldCallNextServiceWhenSessionServiceReturnsNull()
        {
            // arrange
            const int userID = 1019723;

            _sessionServiceMock
                .Setup(x => x.UserID)
                .Returns((int?)null);
            _nextUserContextServiceMock
                .Setup(x => x.GetUserID())
                .Returns(userID);

            // act
            int result = _sut.GetUserID();

            // assert
            result.Should().Be(userID);
            _nextUserContextServiceMock
                .Verify(x => x.GetUserID());
        }

        [Test]
        public void GetUserID_ShouldThrowExceptionWhenSessionServiceReturnsNullAndNextServiceThrowsException()
        {
            // arrange
            var expectedException = new InvalidOperationException();

            _sessionServiceMock
                .Setup(x => x.UserID)
                .Returns((int?)null);
            _nextUserContextServiceMock
                .Setup(x => x.GetUserID())
                .Throws(expectedException);

            Action getUserIDAction = () => _sut.GetUserID();

            // act & assert
            getUserIDAction.ShouldThrow<InvalidOperationException>()
                .Which.Should().Be(expectedException);
        }

        [Test]
        public void GetWorkspaceUserID_ShouldReturnValueIfInjectedSessionServiceReturnsNonNull()
        {
            // arrange
            const int workspaceUserID = 1019723;
            _sessionServiceMock
                .Setup(x => x.WorkspaceUserID)
                .Returns(workspaceUserID);

            // act
            int result = _sut.GetWorkspaceUserID();

            // assert
            result.Should().Be(workspaceUserID);
        }

        [Test]
        public void GetWorkspaceUserID_ShouldCallNextServiceWhenSessionServiceReturnsNull()
        {
            // arrange
            const int workspaceUserID = 1019723;

            _sessionServiceMock
                .Setup(x => x.WorkspaceUserID)
                .Returns((int?)null);
            _nextUserContextServiceMock
                .Setup(x => x.GetWorkspaceUserID())
                .Returns(workspaceUserID);

            // act
            int result = _sut.GetWorkspaceUserID();

            // assert
            result.Should().Be(workspaceUserID);
            _nextUserContextServiceMock
                .Verify(x => x.GetWorkspaceUserID());
        }

        [Test]
        public void GetWorkspaceUserID_ShouldThrowExceptionWhenSessionServiceReturnsNullAndNextServiceThrowsException()
        {
            // arrange
            var expectedException = new InvalidOperationException();

            _sessionServiceMock
                .Setup(x => x.WorkspaceUserID)
                .Returns((int?)null);
            _nextUserContextServiceMock
                .Setup(x => x.GetWorkspaceUserID())
                .Throws(expectedException);

            Action getWorkspaceUserIDAction = () => _sut.GetWorkspaceUserID();

            // act & assert
            getWorkspaceUserIDAction.ShouldThrow<InvalidOperationException>()
                .Which.Should().Be(expectedException);
        }
    }
}
