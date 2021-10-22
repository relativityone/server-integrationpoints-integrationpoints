using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Exceptions;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using kCura.IntegrationPoints.Web.Installers.Context;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Web.Contexts
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class WorkspaceContextIntegrationTests
	{
		private IWindsorContainer _container;
		private Mock<HttpRequestBase> _httpRequestMock;
		private Mock<ISessionService> _sessionServiceMock;


		[SetUp]
		public void SetUp()
		{
			_httpRequestMock = new Mock<HttpRequestBase>();
			_sessionServiceMock = new Mock<ISessionService>();

			_container = CreateIoCContainer();
		}

		[IdentifiedTest("12512dc1-e99d-4585-84e0-a547dfb34df5")]
		[SmokeTest]
		public void ShouldReturnCorrectWorkspaceIdWhenRequestContextContainsData()
		{
			// arrange
			const int requestContextWorkspaceID = 8782;
			const int sessionWorkspaceID = 4232;

			SetupRequestContextMock(requestContextWorkspaceID);
			SetupSessionServiceMock(sessionWorkspaceID);


			TestController sut = _container.Resolve<TestController>();

			// act
			int actualWorkspaceID = sut.GetWorkspaceID();

			// assert
			actualWorkspaceID.Should().Be(requestContextWorkspaceID, "because workspaceId was present in RequestContext");
		}

		[IdentifiedTest("4708e9c8-7e28-414c-815a-db16ffb2e8d2")]
		[SmokeTest]
		public void ShouldReturnCorrectWorkspaceIdWhenRequestContextIsEmptyAndSessionReturnsData()
		{
			// arrange
			const int sessionWorkspaceID = 4232;

			SetupRequestContextMock(workspaceID: null);
			SetupSessionServiceMock(sessionWorkspaceID);

			TestController sut = _container.Resolve<TestController>();

			// act
			int actualWorkspaceID = sut.GetWorkspaceID();

			// assert
			actualWorkspaceID.Should().Be(sessionWorkspaceID, "because session contains this value and RequestContext was empty.");
		}

		[IdentifiedTest("afe02d71-d42b-4db7-96d8-e5f084aa16e7")]
		[SmokeTest]
		public void ShouldThrowExceptionWhenNoWorkspaceContextIsPresent()
		{
			// arrange
			SetupRequestContextMock(workspaceID: null);
			SetupSessionServiceMock(workspaceID: null);

			TestController sut = _container.Resolve<TestController>();

			// act
			Action getWorkspaceIDAction = () => sut.GetWorkspaceID();

			// assert
			getWorkspaceIDAction.ShouldThrow<WorkspaceIdNotFoundException>(
				"because workspace context is not present");
		}

		private void SetupRequestContextMock(int? workspaceID)
		{
			var routeData = new RouteData();
			if (workspaceID.HasValue)
			{
				routeData.Values["workspaceID"] = workspaceID.ToString();
			}

			var requestContextMock = new Mock<RequestContext>();
			requestContextMock
				.Setup(x => x.RouteData)
				.Returns(routeData);

			_httpRequestMock
				.Setup(x => x.RequestContext)
				.Returns(requestContextMock.Object);
		}

		private void SetupSessionServiceMock(int? workspaceID)
		{
			_sessionServiceMock
				.Setup(x => x.WorkspaceID)
				.Returns(workspaceID);
		}

		private void RegisterDependencies(IWindsorContainer container)
		{
			var loggerMock = new Mock<IAPILog>
			{
				DefaultValue = DefaultValue.Mock
			};

			IRegistration[] dependencies =
			{
				Component.For<HttpRequestBase>().Instance(_httpRequestMock.Object),
				Component.For<ISessionService>().Instance(_sessionServiceMock.Object),
				Component.For<IAPILog>().Instance(loggerMock.Object)
			};

			container.Register(dependencies);
		}

		private IWindsorContainer CreateIoCContainer()
		{
			var container = new WindsorContainer();
			container.ConfigureChangingLifestyleFromPerWebRequestToTransientBecausePerWebRequestIsNotResolvableInTests();
			container.AddWorkspaceContext();
			RegisterDependencies(container);
			container.Register(
				Component
					.For<TestController>()
					.LifestyleTransient()
			);

			return container;
		}

		private class TestController : Controller
		{
			private readonly IWorkspaceContext _workspaceContext;

			public TestController(IWorkspaceContext workspaceContext)
			{
				_workspaceContext = workspaceContext;
			}

			public int GetWorkspaceID()
			{
				return _workspaceContext.GetWorkspaceID();
			}
		}
	}
}
