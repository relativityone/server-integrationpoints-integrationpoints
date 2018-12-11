using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Transfer;

namespace kCura.IntegrationPoints.Web.Tests.Services
{
	public class ControllerCustomPageServiceTests
	{
		[Test]
		public void ShouldReturnWorkspaceIfInjectedSessionServiceReturnsWorkspaceId()
		{
			//arrange
			const int workspaceId = 1019723;

			ISessionService sessionService = Substitute.For<ISessionService>();
			sessionService.WorkspaceID.Returns(workspaceId);

			IHelper helper = Substitute.For<IHelper>();

			var service = new ControllerCustomPageService(sessionService, helper);

			//act
			int result = service.GetWorkspaceID();

			//assert
			result.Should().Be(workspaceId);
		}

		[Test]
		public void ShouldReturnZeroIfInjectedSessionServiceThrowsException()
		{
			//arrange
			ISessionService sessionService = Substitute.For<ISessionService>();
			sessionService.WorkspaceID.Throws<Exception>();

			IHelper helper = Substitute.For<IHelper>();

			var service = new ControllerCustomPageService(sessionService, helper);

			//act
			int result = service.GetWorkspaceID();

			//assert
			result.Should().Be(0);
		}

		[Test]
		public void ShouldReturnZeroIfInjectedSessionServiceReturnsZero()
		{
			//arrange
			const int workspaceNotFoundId = 0;

			ISessionService sessionService = Substitute.For<ISessionService>();
			sessionService.WorkspaceID.Returns(workspaceNotFoundId);

			IHelper helper = Substitute.For<IHelper>();

			var service = new ControllerCustomPageService(sessionService, helper);

			//act
			int result = service.GetWorkspaceID();

			//assert
			result.Should().Be(0);
		}
	}
}
