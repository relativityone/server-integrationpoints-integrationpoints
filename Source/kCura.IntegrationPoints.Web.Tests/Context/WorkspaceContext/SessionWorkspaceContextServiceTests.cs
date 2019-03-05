using System;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Context.WorkspaceContext
{
	public class SessionWorkspaceContextServiceTests
	{
		private Mock<ISessionService> _sessionServiceMock;
		private Mock<IWorkspaceContext> _nextWorkspaceContextServiceMock;

		private SessionWorkspaceContextService _sut;

		[SetUp]
		public void SetUp()
		{
			_sessionServiceMock = new Mock<ISessionService>();
			_nextWorkspaceContextServiceMock = new Mock<IWorkspaceContext>();

			_sut = new SessionWorkspaceContextService(
				_sessionServiceMock.Object,
				_nextWorkspaceContextServiceMock.Object
			);
		}

		[Test]
		public void ShouldReturnWorkspaceIfInjectedSessionServiceReturnsWorkspaceId()
		{
			// arrange
			const int workspaceId = 1019723;
			_sessionServiceMock
				.Setup(x => x.WorkspaceID)
				.Returns(workspaceId);

			// act
			int result = _sut.GetWorkspaceId();

			// assert
			result.Should().Be(workspaceId);
		}

		[Test]
		public void ShouldCallNextServiceWhenSessionServiceReturnsNull()
		{
			// arrange
			const int workspaceId = 1019723;

			_sessionServiceMock
				.Setup(x => x.WorkspaceID)
				.Returns((int?)null);
			_nextWorkspaceContextServiceMock
				.Setup(x => x.GetWorkspaceId())
				.Returns(workspaceId);

			// act
			int result = _sut.GetWorkspaceId();

			// assert
			result.Should().Be(workspaceId);
			_nextWorkspaceContextServiceMock
				.Verify(x => x.GetWorkspaceId());
		}

		[Test]
		public void ShouldThrowExceptionWhenSessionServiceReturnsNullAndNextServiceThrowsException()
		{
			// arrange
			var expectedException = new InvalidOperationException();

			_sessionServiceMock
				.Setup(x => x.WorkspaceID)
				.Returns((int?)null);
			_nextWorkspaceContextServiceMock
				.Setup(x => x.GetWorkspaceId())
				.Throws(expectedException);

			Action getWorkspaceAction = () => _sut.GetWorkspaceId();

			// act & assert
			getWorkspaceAction.ShouldThrow<InvalidOperationException>()
				.Which.Should().Be(expectedException);
		}
	}
}
