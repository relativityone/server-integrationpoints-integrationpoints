using System;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider;
using kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider.Services;
using kCura.IntegrationPoints.Web.RelativityServices.Exceptions;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Context.WorkspaceIdProvider
{
	public class WorkspaceIdProviderTests
	{
		[Test]
		public void ShouldReturnWorkspaceIdWhenSingleInjectedServiceReturnsWorkspaceId()
		{
			//arrange
			const int workspaceId = 1019723;
			IWorkspaceService workspaceService1 = Substitute.For<IWorkspaceService>();
			workspaceService1.GetWorkspaceID().Returns(workspaceId);

			IWorkspaceService[] workspaceServices = { workspaceService1 };
			IWorkspaceIdProvider workspaceIdProvider = new Web.Context.WorkspaceIdProvider.WorkspaceIdProvider(workspaceServices);

			//act
			int result = workspaceIdProvider.GetWorkspaceId();

			//assert
			result.Should().Be(workspaceId);
		}

		[Test]
		public void ShouldReturnWorkspaceIdWhenBothTwoInjectedServicesReturnWorkspaceIdFromFirstService()
		{
			//arrange
			const int workspaceId = 1019723;
			IWorkspaceService workspaceService1 = Substitute.For<IWorkspaceService>();
			workspaceService1.GetWorkspaceID().Returns(workspaceId);

			IWorkspaceService workspaceService2 = Substitute.For<IWorkspaceService>();
			workspaceService2.GetWorkspaceID().Returns(workspaceId);

			IWorkspaceService[] workspaceServices = { workspaceService1, workspaceService2 };
			IWorkspaceIdProvider workspaceIdProvider = new Web.Context.WorkspaceIdProvider.WorkspaceIdProvider(workspaceServices);

			//act
			int result = workspaceIdProvider.GetWorkspaceId();

			//assert
			result.Should().Be(workspaceId);
			workspaceService1.Received(1).GetWorkspaceID();
			workspaceService2.DidNotReceive().GetWorkspaceID();
		}

		[Test]
		public void ShouldThrowWhenNoServiceInjected()
		{
			//arrange
			var workspaceServices = new IWorkspaceService[] {};
			IWorkspaceIdProvider workspaceIdProvider = new Web.Context.WorkspaceIdProvider.WorkspaceIdProvider(workspaceServices);

			//act
			Action act = () => workspaceIdProvider.GetWorkspaceId();

			//assert
			act.ShouldThrow<WorkspaceIdNotFoundException>();
		}

		[Test]
		public void ShouldThrowWhenBothTwoInjectedServicesReturnZero()
		{
			//arrange
			const int workspaceNotFoundId = 0;
			IWorkspaceService workspaceService1 = Substitute.For<IWorkspaceService>();
			workspaceService1.GetWorkspaceID().Returns(workspaceNotFoundId);

			IWorkspaceService workspaceService2 = Substitute.For<IWorkspaceService>();
			workspaceService2.GetWorkspaceID().Returns(workspaceNotFoundId);

			IWorkspaceService[] workspaceServices = { workspaceService1, workspaceService2 };
			IWorkspaceIdProvider workspaceIdProvider = new Web.Context.WorkspaceIdProvider.WorkspaceIdProvider(workspaceServices);

			//act
			Action act = () => workspaceIdProvider.GetWorkspaceId();

			//assert
			act.ShouldThrow<WorkspaceIdNotFoundException>();
		}

		[Test]
		public void ShouldReturnWorkspaceIdWhenOneInjectedServiceReturnsZeroAndOtherReturnsWorkspaceId()
		{
			//arrange
			const int workspaceId = 1019723;
			const int workspaceNotFoundId = 0;
			IWorkspaceService workspaceService1 = Substitute.For<IWorkspaceService>();
			workspaceService1.GetWorkspaceID().Returns(workspaceNotFoundId);

			IWorkspaceService workspaceService2 = Substitute.For<IWorkspaceService>();
			workspaceService2.GetWorkspaceID().Returns(workspaceId);

			IWorkspaceService[] workspaceServices = { workspaceService1, workspaceService2 };
			IWorkspaceIdProvider workspaceIdProvider = new Web.Context.WorkspaceIdProvider.WorkspaceIdProvider(workspaceServices);

			//act
			int result = workspaceIdProvider.GetWorkspaceId();

			//assert
			result.Should().Be(workspaceId);
		}
	}
}
