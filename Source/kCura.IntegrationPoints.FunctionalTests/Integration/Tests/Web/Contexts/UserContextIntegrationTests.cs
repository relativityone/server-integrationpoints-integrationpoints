using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.UserContext;
using kCura.IntegrationPoints.Web.Context.UserContext.Exceptions;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using kCura.IntegrationPoints.Web.Installers.Context;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Integration.Assertions;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Web.Contexts
{
    [TestFixture]
    [Feature.DataTransfer.IntegrationPoints]
    public class UserContextIntegrationTests : TestsBase
    {
        private Mock<HttpRequestBase> _httpRequestMock;
        private Mock<ISessionService> _sessionServiceMock;


        [SetUp]
        public void Setup()
        {
            _httpRequestMock = new Mock<HttpRequestBase>();
            _sessionServiceMock = new Mock<ISessionService>();
            PrepareContainer();
        }

        [IdentifiedTest("e0b4f1a1-1d3a-4a81-9a6a-ec9b6466f488")]
        [Feature.InfrastructureOperations.SmokeTest]
        public void GetUserId_ShouldReturnCorrectValueWhenRequestContextContainsData()
        {
            // Arrange
            const int requestContextUserId = 8782;
            const int sessionUserId = 4232;

            SetupRequestContextMockUserId(requestContextUserId);
            SetupSessionServiceMockUserId(sessionUserId);

            TestController sut = Container.Resolve<TestController>();

            // Act
            int actualUserId = sut.GetUserId();

            // Assert
            actualUserId.Should().Be(requestContextUserId, "because userId was present in headers");
        }

        [IdentifiedTest("9172e58f-b9cb-49ea-a206-f93dcdc7cb44")]
        [Feature.InfrastructureOperations.SmokeTest]
        public void GetUserId_ShouldReturnCorrectValueWhenRequestContextIsEmptyAndSessionReturnsData()
        {
            // Arrange
            const int sessionUserId = 4232;

            SetupRequestContextMockUserId(userId: null);
            SetupSessionServiceMockUserId(sessionUserId);

            TestController sut = Container.Resolve<TestController>();

            // Act
            int actualUserId = sut.GetUserId();

            // Assert
            actualUserId.Should().Be(sessionUserId, "because session contains this value and RequestContext was empty.");
        }

        [IdentifiedTest("ae5c0763-1401-456c-8754-3b87286aae71")]
        [Feature.InfrastructureOperations.SmokeTest]
        public void GetUserId_ShouldThrowExceptionWhenNoWorkspaceContextIsPresent()
        {
            // Arrange
            SetupRequestContextMockUserId(userId: null);
            SetupSessionServiceMockUserId(userId: null);

            TestController sut = Container.Resolve<TestController>();

            // Act
            Action getUserIdAction = () => sut.GetUserId();

            // Assert
            getUserIdAction.ShouldThrow<UserContextNotFoundException>(
                "because user context is not present");
        }

        [IdentifiedTest("af81317c-0af7-4951-881c-13627415f2ec")]
        [Feature.InfrastructureOperations.SmokeTest]
        public void GetWorkspaceUserId_ShouldReturnCorrectValueWhenRequestContextContainsData()
        {
            // Arrange
            const int requestContextWorkspaceUserId = 8782;
            const int sessionWorkspaceUserId = 4232;

            SetupRequestContextMockWorkspaceUserId(requestContextWorkspaceUserId);
            SetupSessionServiceMockWorkspaceUserId(sessionWorkspaceUserId);

            TestController sut = Container.Resolve<TestController>();

            // Act
            int actualWorkspaceUserId = sut.GetWorkspaceUserId();

            // Assert
            actualWorkspaceUserId.Should().Be(requestContextWorkspaceUserId, "because workspaceUserId was present in headers");
        }

        [IdentifiedTest("84ce667a-407e-4417-a646-4bb49172d6e3")]
        [Feature.InfrastructureOperations.SmokeTest]
        public void GetWorkspaceUserId_ShouldReturnCorrectValueWhenRequestContextIsEmptyAndSessionReturnsData()
        {
            // Arrange
            const int sessionWorkspaceUserId = 4232;

            SetupRequestContextMockWorkspaceUserId(workspaceUserId: null);
            SetupSessionServiceMockWorkspaceUserId(sessionWorkspaceUserId);

            TestController sut = Container.Resolve<TestController>();

            // Act
            int actualWorkspaceUserId = sut.GetWorkspaceUserId();

            // Assert
            actualWorkspaceUserId.Should().Be(sessionWorkspaceUserId, "because session contains this value and RequestContext was empty.");
        }

        [IdentifiedTest("e872e124-1030-4028-902b-1dbde41b1a29")]
        [Feature.InfrastructureOperations.SmokeTest]
        public void GetWorkspaceUserId_ShouldThrowExceptionWhenNoWorkspaceContextIsPresent()
        {
            // Arrange
            SetupRequestContextMockWorkspaceUserId(workspaceUserId: null);
            SetupSessionServiceMockWorkspaceUserId(workspaceUserId: null);

            TestController sut = Container.Resolve<TestController>();

            // Act
            Action getWorkspaceUserIdAction = () => sut.GetWorkspaceUserId();

            // Assert
            getWorkspaceUserIdAction.ShouldThrow<UserContextNotFoundException>(
                "because user context is not present");
        }

        private void SetupRequestContextMockUserId(int? userId)
        {
            SetupRequestContextMock("X-IP-USERId", userId);
        }

        private void SetupRequestContextMockWorkspaceUserId(int? workspaceUserId)
        {
            SetupRequestContextMock("X-IP-CASEUSERId", workspaceUserId);
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

        private void SetupSessionServiceMockUserId(int? userId)
        {
            _sessionServiceMock
                .Setup(x => x.UserID)
                .Returns(userId);
        }

        private void SetupSessionServiceMockWorkspaceUserId(int? workspaceUserId)
        {
            _sessionServiceMock
                .Setup(x => x.WorkspaceUserID)
                .Returns(workspaceUserId);
        }

        private void PrepareContainer()
        {
            Container.ConfigureChangingLifestyleFromPerWebRequestToTransientBecausePerWebRequestIsNotResolvableInTests();
            Container.AddUserContext();

            Container.Register(Component.For<HttpRequestBase>().Instance(_httpRequestMock.Object));
            Container.Register(Component.For<ISessionService>().Instance(_sessionServiceMock.Object));
            
            Container.Register(
                Component
                    .For<TestController>()
                    .LifestyleTransient()
            );
        }
        
        private class TestController : Controller
        {
            private readonly IUserContext _userContext;

            public TestController(IUserContext userContext)
            {
                _userContext = userContext;
            }

            public int GetUserId()
            {
                return _userContext.GetUserID();
            }

            public int GetWorkspaceUserId()
            {
                return _userContext.GetWorkspaceUserID();
            }
        }
    }
}
