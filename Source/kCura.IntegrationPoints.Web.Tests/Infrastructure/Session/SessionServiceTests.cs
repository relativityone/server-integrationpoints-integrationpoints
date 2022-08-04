using FluentAssertions;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;

namespace kCura.IntegrationPoints.Web.Tests.Infrastructure.Session
{
    [TestFixture, Category("Unit")]
    public class SessionServiceTests
    {
        private SessionService _sut;
        private Mock<ICPHelper> _helperMock;
        private Mock<IAPILog> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            InitializeLoggerMock();
            _helperMock = new Mock<ICPHelper>();

            _sut = new SessionService(
                _helperMock.Object,
                _loggerMock.Object
            );
        }

        [Test]
        public void WorkspaceID_ShouldReturnCorrectValue()
        {
            // arrange
            const int expectedWorkspaceID = 43531921;
            _helperMock
                .Setup(x => x.GetActiveCaseID())
                    .Returns(expectedWorkspaceID);

            // act
            int? workspaceID = _sut.WorkspaceID;

            // assert
            workspaceID.Should().Be(expectedWorkspaceID);
        }

        [Test]
        public void WorkspaceID_ShouldReturnNullWhenExceptionWasThrown()
        {
            // arrange
            var expectedException = new InvalidOperationException();
            _helperMock
                .Setup(x => x.GetActiveCaseID())
                .Throws(expectedException);

            // act
            int? workspaceID = _sut.WorkspaceID;

            // assert
            workspaceID.Should().BeNull("because helper threw an exception.");
        }

        [Test]
        public void WorkspaceID_ShouldLogWarningWhenExceptionWasThrown()
        {
            // arrange
            var expectedException = new InvalidOperationException();
            _helperMock
                .Setup(x => x.GetActiveCaseID())
                .Throws(expectedException);

            // act
            int? workspaceID = _sut.WorkspaceID;

            // assert
            _loggerMock.Verify(x =>
                x.LogWarning(expectedException, "SessionService failed when executing WorkspaceID")
            );
        }

        [Test]
        public void UserID_ShouldReturnCorrectValue()
        {
            // arrange
            const int expectedUserID = 2983231;

            _helperMock
                .Setup(x => x.GetAuthenticationManager().UserInfo.ArtifactID)
                    .Returns(expectedUserID);

            // act
            int? userID = _sut.UserID;

            // assert
            userID.Should().Be(expectedUserID);
        }

        [Test]
        public void UserID_ShouldReturnNullWhenExceptionWasThrown()
        {
            // arrange
            var expectedException = new InvalidOperationException();
            _helperMock
                .Setup(x => x.GetAuthenticationManager().UserInfo.ArtifactID)
                .Throws(expectedException);

            // act
            int? userID = _sut.UserID;

            // assert
            userID.Should().BeNull();
        }

        [Test]
        public void UserID_ShouldLogWarningWhenExceptionWasThrown()
        {
            // arrange
            var expectedException = new InvalidOperationException();
            _helperMock
                .Setup(x => x.GetAuthenticationManager().UserInfo.ArtifactID)
                .Throws(expectedException);

            // act
            int? userID = _sut.UserID;

            // assert
            _loggerMock.Verify(x =>
                x.LogWarning(expectedException, "SessionService failed when executing UserID")
            );
        }

        [Test]
        public void WorkspaceUserID_ShouldReturnCorrectValue()
        {
            // arrange
            const int expectedWorkspaceUserID = 6735983;
            _helperMock
                .Setup(x => x.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID)
                .Returns(expectedWorkspaceUserID);

            // act
            int? workspaceUserId = _sut.WorkspaceUserID;

            // assert
            workspaceUserId.Should().Be(expectedWorkspaceUserID);
        }

        [Test]
        public void WorkspaceUserID_ShouldReturnNullWhenExceptionWasThrown()
        {
            // arrange
            var expectedException = new InvalidOperationException();
            _helperMock
                .Setup(x => x.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID)
                .Throws(expectedException);

            // act
            int? workspaceUserId = _sut.WorkspaceUserID;

            // assert
            workspaceUserId.Should().BeNull();
        }

        [Test]
        public void WorkspaceUserID_ShouldLogWarningWhenExceptionWasThrown()
        {
            // arrange
            var expectedException = new InvalidOperationException();
            _helperMock
                .Setup(x => x.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID)
                .Throws(expectedException);

            // act
            int? workspaceUserId = _sut.WorkspaceUserID;

            // assert
            _loggerMock.Verify(x =>
                x.LogWarning(expectedException, "SessionService failed when executing WorkspaceUserID")
            );
        }

        private void InitializeLoggerMock()
        {
            _loggerMock = new Mock<IAPILog>();
            _loggerMock
                .Setup(x => x.ForContext<SessionService>())
                .Returns(_loggerMock.Object);
        }
    }
}
