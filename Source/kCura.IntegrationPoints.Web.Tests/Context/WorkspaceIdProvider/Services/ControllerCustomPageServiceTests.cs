using System;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Services;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Context.WorkspaceIdProvider.Services
{
	public class ControllerCustomPageServiceTests
	{
		private ISessionService _sessionServiceMock;
		private ControllerCustomPageService _sut;

		[SetUp]
		public void SetUp()
		{
			_sessionServiceMock = Substitute.For<ISessionService>();
			IAPILog logger= Substitute.For<IAPILog>();

			_sut = new ControllerCustomPageService(_sessionServiceMock, logger);
		}

		[Test]
		public void ShouldReturnWorkspaceIfInjectedSessionServiceReturnsWorkspaceId()
		{
			//arrange
			const int workspaceId = 1019723;
			_sessionServiceMock.WorkspaceID.Returns(workspaceId);
			
			//act
			int result = _sut.GetWorkspaceID();

			//assert
			result.Should().Be(workspaceId);
		}

		[Test]
		public void ShouldReturnZeroIfInjectedSessionServiceThrowsException()
		{
			//arrange
			_sessionServiceMock.WorkspaceID.Throws<Exception>();
			
			//act
			int result = _sut.GetWorkspaceID();

			//assert
			result.Should().Be(0);
		}

		[Test]
		public void ShouldReturnZeroIfInjectedSessionServiceReturnsZero()
		{
			//arrange
			const int workspaceNotFoundId = 0;
			_sessionServiceMock.WorkspaceID.Returns(workspaceNotFoundId);

			//act
			int result = _sut.GetWorkspaceID();

			//assert
			result.Should().Be(0);
		}
	}
}
