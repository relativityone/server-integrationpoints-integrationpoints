using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Exceptions;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using kCura.IntegrationPoints.Web.Installers.Context;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;

namespace kCura.IntegrationPoints.Web.Tests.Integration.Context
{
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

		[Test]
		[SmokeTest]
		public void ShouldReturnCorrectWorkspaceIdWhenRequestContextContainsData()
		{
			// arrange
			const int requestContextWorkspaceId = 8782;
			const int sessionWorkspaceId = 4232;

			SetupRequestContextMock(requestContextWorkspaceId);
			SetupSessionServiceMock(sessionWorkspaceId);


			TestController sut = _container.Resolve<TestController>();

			// act
			int actualWorkspaceId = sut.GetWorkspaceId();

			// assert
			actualWorkspaceId.Should().Be(requestContextWorkspaceId, "because workspaceId was present in RequestContext");
		}

		[Test]
		[SmokeTest]
		public void ShouldReturnCorrectWorkspaceIdWhenRequestContextIsEmptyAndSessionReturnsData()
		{
			// arrange
			const int sessionWorkspaceId = 4232;

			SetupRequestContextMock(workspaceId: null);
			SetupSessionServiceMock(sessionWorkspaceId);

			TestController sut = _container.Resolve<TestController>();

			// act
			int actualWorkspaceId = sut.GetWorkspaceId();

			// assert
			actualWorkspaceId.Should().Be(sessionWorkspaceId, "because session contains this value and RequestContext was empty.");
		}

		[Test]
		[SmokeTest]
		public void ShouldThrowExceptionWhenNoWorkspaceContextIsPresent()
		{
			// arrange
			SetupRequestContextMock(workspaceId: null);
			SetupSessionServiceMock(workspaceId: null);

			TestController sut = _container.Resolve<TestController>();

			// act
			Action getWorkspaceIdAction = () => sut.GetWorkspaceId();

			// assert
			getWorkspaceIdAction.ShouldThrow<WorkspaceIdNotFoundException>(
				"because workspace context is not present");
		}

		private void SetupRequestContextMock(int? workspaceId)
		{
			var routeData = new RouteData();
			if (workspaceId.HasValue)
			{
				routeData.Values["workspaceID"] = workspaceId.ToString();
			}

			var requestContextMock = new Mock<RequestContext>();
			requestContextMock
				.Setup(x => x.RouteData)
				.Returns(routeData);

			_httpRequestMock
				.Setup(x => x.RequestContext)
				.Returns(requestContextMock.Object);
		}

		private void SetupSessionServiceMock(int? workspaceId)
		{
			_sessionServiceMock
				.Setup(x => x.WorkspaceID)
				.Returns(workspaceId);
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
			container.ChangeLifestyleFromPerWebRequestToTransientInNewRegistrations(); // we cannot resolve PerWebRequest object in tests
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

			public int GetWorkspaceId()
			{
				return _workspaceContext.GetWorkspaceID();
			}
		}
	}
}
