using System;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.UserContext;
using kCura.IntegrationPoints.Web.Context.UserContext.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Context.UserContext
{
    [TestFixture, Category("Unit")]
    public class NotFoundUserContextServiceTests
    {
        private Mock<IAPILog> _loggerMock;
        private NotFoundUserContextService _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _loggerMock
                .Setup(x => x.ForContext<NotFoundUserContextService>())
                .Returns(_loggerMock.Object);

            _sut = new NotFoundUserContextService(_loggerMock.Object);
        }

        [Test]
        public void GetUserID_ShouldThrowExceptionAndLogWarning()
        {
            // arrange
            Action getUserIDAction = () => _sut.GetUserID();

            // act & assert
            getUserIDAction
                .ShouldThrow<UserContextNotFoundException>("because user context was not found");
            _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()));
        }

        [Test]
        public void GetWorkspaceUserID_ShouldThrowExceptionAndLogWarning()
        {
            // arrange
            Action getWorkspaceUserIDAction = () => _sut.GetWorkspaceUserID();

            // act & assert
            getWorkspaceUserIDAction
                .ShouldThrow<UserContextNotFoundException>("because user context was not found");
            _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()));
        }
    }
}
