using System;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Context.WorkspaceContext
{
    [TestFixture, Category("Unit")]
    public class NotFoundWorkspaceContextServiceTests
    {
        private Mock<IAPILog> _loggerMock;
        private NotFoundWorkspaceContextService _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _loggerMock
                .Setup(x => x.ForContext<NotFoundWorkspaceContextService>())
                .Returns(_loggerMock.Object);

            _sut = new NotFoundWorkspaceContextService(_loggerMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionAndLogWarning()
        {
            // arrange
            Action getWorkspaceIDAction = () => _sut.GetWorkspaceID();

            // act & assert
            getWorkspaceIDAction
                .ShouldThrow<WorkspaceIdNotFoundException>("because workspace context was not found");
            _loggerMock.Verify(x=>x.LogWarning(It.IsAny<string>()));
        }
    }
}
