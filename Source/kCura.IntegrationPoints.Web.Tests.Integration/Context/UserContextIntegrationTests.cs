using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using kCura.IntegrationPoints.Web.Installers.Context;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Web.Context.UserContext;
using kCura.IntegrationPoints.Web.Context.UserContext.Exceptions;

namespace kCura.IntegrationPoints.Web.Tests.Integration.Context
{
	[TestFixture]
	public class UserContextIntegrationTests
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
		public void GetUserID_ShouldReturnCorrectValueWhenRequestContextContainsData()
		{
			// arrange
			const int requestContextUserID = 8782;
			const int sessionUserID = 4232;

			SetupRequestContextMockUserID(requestContextUserID);
			SetupSessionServiceMockUserID(sessionUserID);

			TestController sut = _container.Resolve<TestController>();

			// act
			int actualUserID = sut.GetUserID();

			// assert
			actualUserID.Should().Be(requestContextUserID, "because userId was present in headers");
		}

		[Test]
		[SmokeTest]
		public void GetUserID_ShouldReturnCorrectValueWhenRequestContextIsEmptyAndSessionReturnsData()
		{
			// arrange
			const int sessionUserID = 4232;

			SetupRequestContextMockUserID(userID: null);
			SetupSessionServiceMockUserID(sessionUserID);

			TestController sut = _container.Resolve<TestController>();

			// act
			int actualUserID = sut.GetUserID();

			// assert
			actualUserID.Should().Be(sessionUserID, "because session contains this value and RequestContext was empty.");
		}

		[Test]
		[SmokeTest]
		public void GetUserID_ShouldThrowExceptionWhenNoWorkspaceContextIsPresent()
		{
			// arrange
			SetupRequestContextMockUserID(userID: null);
			SetupSessionServiceMockUserID(userID: null);

			TestController sut = _container.Resolve<TestController>();

			// act
			Action getUserIDAction = () => sut.GetUserID();

			// assert
			getUserIDAction.ShouldThrow<UserContextNotFoundException>(
				"because user context is not present");
		}

		[Test]
		[SmokeTest]
		public void GetWorkspaceUserID_ShouldReturnCorrectValueWhenRequestContextContainsData()
		{
			// arrange
			const int requestContextWorkspaceUserID = 8782;
			const int sessionWorkspaceUserID = 4232;

			SetupRequestContextMockWorkspaceUserID(requestContextWorkspaceUserID);
			SetupSessionServiceMockWorkspaceUserID(sessionWorkspaceUserID);

			TestController sut = _container.Resolve<TestController>();

			// act
			int actualWorkspaceUserID = sut.GetWorkspaceUserID();

			// assert
			actualWorkspaceUserID.Should().Be(requestContextWorkspaceUserID, "because workspaceUserId was present in headers");
		}

		[Test]
		[SmokeTest]
		public void GetWorkspaceUserID_ShouldReturnCorrectValueWhenRequestContextIsEmptyAndSessionReturnsData()
		{
			// arrange
			const int sessionWorkspaceUserID = 4232;

			SetupRequestContextMockWorkspaceUserID(workspaceUserID: null);
			SetupSessionServiceMockWorkspaceUserID(sessionWorkspaceUserID);

			TestController sut = _container.Resolve<TestController>();

			// act
			int actualWorkspaceUserID = sut.GetWorkspaceUserID();

			// assert
			actualWorkspaceUserID.Should().Be(sessionWorkspaceUserID, "because session contains this value and RequestContext was empty.");
		}

		[Test]
		[SmokeTest]
		public void GetWorkspaceUserID_ShouldThrowExceptionWhenNoWorkspaceContextIsPresent()
		{
			// arrange
			SetupRequestContextMockWorkspaceUserID(workspaceUserID: null);
			SetupSessionServiceMockWorkspaceUserID(workspaceUserID: null);

			TestController sut = _container.Resolve<TestController>();

			// act
			Action getWorkspaceUserIDAction = () => sut.GetWorkspaceUserID();

			// assert
			getWorkspaceUserIDAction.ShouldThrow<UserContextNotFoundException>(
				"because user context is not present");
		}

		private void SetupRequestContextMockUserID(int? userID)
		{
			SetupRequestContextMock("X-IP-USERID", userID);
		}

		private void SetupRequestContextMockWorkspaceUserID(int? workspaceUserID)
		{
			SetupRequestContextMock("X-IP-CASEUSERID", workspaceUserID);
		}

		private void SetupRequestContextMock(string key, int? value)
		{
			var headers = new NameValueCollection();
			if (value.HasValue)
			{
				headers.Add(key, value.ToString());
			}

			_httpRequestMock.Setup(x => x.Headers).Returns(headers);
		}

		private void SetupSessionServiceMockUserID(int? userID)
		{
			_sessionServiceMock
				.Setup(x => x.UserID)
				.Returns(userID);
		}

		private void SetupSessionServiceMockWorkspaceUserID(int? workspaceUserID)
		{
			_sessionServiceMock
				.Setup(x => x.WorkspaceUserID)
				.Returns(workspaceUserID);
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
			container.AddUserContext();
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
			private readonly IUserContext _userContext;

			public TestController(IUserContext userContext)
			{
				_userContext = userContext;
			}

			public int GetUserID()
			{
				return _userContext.GetUserID();
			}

			public int GetWorkspaceUserID()
			{
				return _userContext.GetWorkspaceUserID();
			}
		}
	}
}
