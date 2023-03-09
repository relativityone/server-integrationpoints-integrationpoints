using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Hosting;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
    [TestFixture, Category("Unit")]
    public class JobControllerTests : TestBase
    {
        private IAuditManager _auditManager;
        private ICPHelper _helper;
        private IIntegrationPointService _integrationPointService;
        private IManagerFactory _managerFactory;
        private IRepositoryFactory _repositoryFactory;
        private IRelativityAuditRepository _auditRepository;
        private IPermissionRepository _permissionRepository;
        private JobController _instance;
        private JobController.Payload _payload;
        private const int _INTEGRATION_POINT_ARTIFACT_ID = 1003663;
        private const int _USERID = 9;
        private const int _WORKSPACE_ARTIFACT_ID = 1020530;
        private const string _RETRY_AUDIT_MESSAGE = "Retry error was attempted.";
        private const string _RUN_AUDIT_MESSAGE = "Transfer was attempted.";
        private const string _STOP_AUDIT_MESSAGE = "Stop transfer was attempted.";
        private const string _EMPTY_SECURED_CONFIG = "{}";
        private readonly string _userIdString = _USERID.ToString();

        [SetUp]
        public override void SetUp()
        {
            _payload = new JobController.Payload { AppId = _WORKSPACE_ARTIFACT_ID, ArtifactId = _INTEGRATION_POINT_ARTIFACT_ID };

            _integrationPointService = Substitute.For<IIntegrationPointService>();
            _helper = Substitute.For<ICPHelper>();
            _auditManager = Substitute.For<IAuditManager>();
            _managerFactory = Substitute.For<IManagerFactory>();
            _auditRepository = Substitute.For<IRelativityAuditRepository>();

            _helper.GetActiveCaseID().Returns(_WORKSPACE_ARTIFACT_ID);
            _managerFactory.CreateAuditManager(_WORKSPACE_ARTIFACT_ID).Returns(_auditManager);
            _auditManager.RelativityAuditRepository.Returns(_auditRepository);

            _permissionRepository = Substitute.For<IPermissionRepository>();
            _permissionRepository.UserHasArtifactTypePermissions(Arg.Any<Guid>(), Arg.Any<IEnumerable<ArtifactPermission>>()).Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(Arg.Any<Guid>(), Arg.Any<ArtifactPermission>()).Returns(true);

            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _repositoryFactory.GetPermissionRepository(_WORKSPACE_ARTIFACT_ID).Returns(_permissionRepository);

            IAPILog log = Substitute.For<IAPILog>();

            _instance = new JobController(
                _repositoryFactory,
                _managerFactory,
                _integrationPointService,
                log)
            {
                Request = new HttpRequestMessage()
            };
            _instance.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCase(null)]
        [TestCase(1000)]
        public void ControllerDoesNotHaveUserIdInTheHeaderWhenTryingToSubmitPushingJob_ExpectBadRequest(int? federatedInstanceArtifactId)
        {
            // Arrange
            var integrationPoint = new IntegrationPointSlimDto()
            {
                SecuredConfiguration = _EMPTY_SECURED_CONFIG
            };
            const string expectedErrorMessage = @"Unable to determine the user id. Please contact your system administrator.";

            Exception exception = new Exception(expectedErrorMessage);

            _integrationPointService.ReadSlim(_INTEGRATION_POINT_ARTIFACT_ID).Returns(integrationPoint);

            _integrationPointService.When(
                service => service.RunIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, 0))
                .Throw(exception);

            // Act
            HttpResponseMessage response = _instance.Run(_payload);

            // Assert
            _auditRepository.Received(1)
                .CreateAuditRecord(_payload.ArtifactId,
                Arg.Is<AuditElement>(audit => audit.AuditMessage == _RUN_AUDIT_MESSAGE));
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            JobActionResult body = response.Content.ReadAsAsync<JobActionResult>().GetAwaiter().GetResult();

            body.Errors.Should().Contain(expectedErrorMessage);
        }

        [TestCase(null)]
        [TestCase(1000)]
        public void KeplerCallThrowsException(int? federatedInstanceArtifactId)
        {
            // Arrange
            var claims = new List<Claim>(1)
            {
                new Claim("rel_uai", _userIdString)
            };
            _instance.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            const string expectedErrorMessage1 = "ABC";
            const string expectedErrorMessage2 = "456";
            const string expectedErrorMessage3 = "123";

            List<string> expectedErrorMessages = new List<string> { expectedErrorMessage1, expectedErrorMessage2, expectedErrorMessage3 };

            AggregateException exceptionToBeThrown =
                new AggregateException(expectedErrorMessage1, new AccessViolationException(expectedErrorMessage3), new Exception(expectedErrorMessage2));

            var integrationPoint = new IntegrationPointSlimDto()
            {
                SecuredConfiguration = _EMPTY_SECURED_CONFIG
            };

            _integrationPointService.ReadSlim(_INTEGRATION_POINT_ARTIFACT_ID).Returns(integrationPoint);

            _integrationPointService.When(
                service => service.RunIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, _USERID))
                .Throw(exceptionToBeThrown);

            // Act
            HttpResponseMessage response = _instance.Run(_payload);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            JobActionResult body = response.Content.ReadAsAsync<JobActionResult>().GetAwaiter().GetResult();

            body.Errors.Should().Contain(expectedErrorMessage1)
                .And.Contain(expectedErrorMessage2);
        }

        [TestCase(null)]
        [TestCase(1000)]
        public void ControllerDoesNotHaveUserIdInTheHeaderWhenTryingToSubmitNormalJob_ExpectNoError(int? federatedInstanceArtifactId)
        {
            // Arrange
            var integrationPoint = new IntegrationPointSlimDto()
            {
                SecuredConfiguration = _EMPTY_SECURED_CONFIG
            };

            _integrationPointService.ReadSlim(_INTEGRATION_POINT_ARTIFACT_ID).Returns(integrationPoint);
            _instance.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(0)));

            // Act
            HttpResponseMessage response = _instance.Run(_payload);

            // Assert
            _integrationPointService.Received(1).RunIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, 0);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public void Run_ShouldCheckRdoPermissions()
        {
            // Arrange
            var integrationPoint = new IntegrationPointSlimDto();

            _integrationPointService.ReadSlim(_INTEGRATION_POINT_ARTIFACT_ID).Returns(integrationPoint);
            _instance.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(0)));

            // Act
            _instance.Run(_payload);

            // Assert
            AssertRDOsPermissions();
        }

        [Test]
        public void Retry_ShouldCheckRdoPermissions()
        {
            // Arrange
            var integrationPoint = new IntegrationPointSlimDto();

            _integrationPointService.ReadSlim(_INTEGRATION_POINT_ARTIFACT_ID).Returns(integrationPoint);
            _instance.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(0)));

            // Act
            _instance.Run(_payload);

            // Assert
            AssertRDOsPermissions();
        }

        [TestCase(null)]
        [TestCase(1000)]
        public void NonRelativityProviderCall(int? federatedInstanceArtifactId)
        {
            // Arrange
            var claims = new List<Claim>(1)
            {
                new Claim("rel_uai", _userIdString)
            };

            var integrationPoint = new IntegrationPointSlimDto()
            {
                SecuredConfiguration = _EMPTY_SECURED_CONFIG
            };

            _integrationPointService.ReadSlim(_INTEGRATION_POINT_ARTIFACT_ID).Returns(integrationPoint);
            _instance.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Act
            HttpResponseMessage response = _instance.Run(_payload);

            // Assert
            _integrationPointService.Received(1).RunIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, _USERID);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public void RetryJob_UserIdExists_Succeeds_Test()
        {
            // Arrange
            var claims = new List<Claim>(1)
            {
                new Claim("rel_uai", _userIdString)
            };

            var integrationPoint = new IntegrationPointDto()
            {
                DestinationConfiguration = new ImportSettings(),
                SecuredConfiguration = _EMPTY_SECURED_CONFIG
            };

            _instance.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            _integrationPointService.Read(_INTEGRATION_POINT_ARTIFACT_ID).Returns(integrationPoint);

            // Act
            HttpResponseMessage response = _instance.Retry(_payload);

            // Assert
            _auditRepository.Received(1)
                            .CreateAuditRecord(_payload.ArtifactId,
                            Arg.Is<AuditElement>(audit => audit.AuditMessage == _RETRY_AUDIT_MESSAGE));
            _integrationPointService.Received(1).RetryIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, _USERID, switchToAppendOverlayMode: false);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public void RetryJob_UserIdDoesNotExist_IntegrationPointServiceThrowsError_Test()
        {
            // Arrange
            var integrationPoint = new IntegrationPointDto()
            {
                DestinationConfiguration = new ImportSettings(),
                SecuredConfiguration = _EMPTY_SECURED_CONFIG
            };

            _instance.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(0)));
            var exception = new Exception(Core.Constants.IntegrationPoints.NO_USERID);
            _integrationPointService.When(x => x.RetryIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, 0, switchToAppendOverlayMode: false))
                .Throw(exception);
            _integrationPointService.Read(_INTEGRATION_POINT_ARTIFACT_ID).Returns(integrationPoint);

            // Act
            HttpResponseMessage response = _instance.Retry(_payload);

            // Assert
            _auditRepository.Received(1)
                .CreateAuditRecord(_payload.ArtifactId,
                Arg.Is<AuditElement>(audit => audit.AuditMessage == _RETRY_AUDIT_MESSAGE));
            _integrationPointService.Received(1).RetryIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, 0, switchToAppendOverlayMode: false);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            JobActionResult body = response.Content.ReadAsAsync<JobActionResult>().GetAwaiter().GetResult();

            body.Errors.Should().Contain(Core.Constants.IntegrationPoints.NO_USERID);
        }

        [Test]
        public void Stop_GoldFlow()
        {
            // Arrange
            // Act
            HttpResponseMessage response = _instance.Stop(_payload);

            // Assert

            _auditRepository.Received(1)
                .CreateAuditRecord(_payload.ArtifactId,
                Arg.Is<AuditElement>(audit => audit.AuditMessage == _STOP_AUDIT_MESSAGE));

            _integrationPointService
                .Received(1)
                .MarkIntegrationPointToStopJobs(_payload.AppId, _payload.ArtifactId);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "The HTTPStatusCode should be OK");

            JobActionResult body = response.Content.ReadAsAsync<JobActionResult>().GetAwaiter().GetResult();

            body.IsValid.Should().BeTrue();
        }

        [Test]
        public void Stop_AggregateExceptionThrown_ResponseIsCorrect()
        {
            // Arrange
            const string exceptionOne = "Exception One";
            const string exceptionTwo = "Exception Two";
            const string aggregateExceptionMessage = "Topmost Message";
            var aggregateException = new AggregateException(aggregateExceptionMessage, new[] { new Exception(exceptionOne), new Exception(exceptionTwo) });
            string expectedErrorMessage = $"{aggregateException.Message} : {String.Join(",", new[] { exceptionOne, exceptionTwo })}";
            ErrorDTO error = new ErrorDTO
            {
                Message = expectedErrorMessage,
                Source = Core.Constants.IntegrationPoints.APPLICATION_NAME,
                WorkspaceId = _WORKSPACE_ARTIFACT_ID
            };

            IErrorManager errorManager = Substitute.For<IErrorManager>();

            _integrationPointService
                .When(x => x.MarkIntegrationPointToStopJobs(_payload.AppId, _payload.ArtifactId))
                .Throw(aggregateException);
            _managerFactory.CreateErrorManager().Returns(errorManager);
            errorManager.Create(Arg.Is<IEnumerable<ErrorDTO>>(x => x.First().Equals(error)));

            // Act
            HttpResponseMessage response = _instance.Stop(_payload);

            // Assert
            _integrationPointService
                .Received(1)
                .MarkIntegrationPointToStopJobs(_payload.AppId, _payload.ArtifactId);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "The HTTPStatusCode should be BadRequest");

            errorManager.Received(1).Create(Arg.Is<IEnumerable<ErrorDTO>>(x => x.First().Equals(error)));
        }

        [Test]
        public void Stop_ExceptionThrown_ResponseIsCorrect()
        {
            // Arrange
            var exception = new Exception("exception message");
            ErrorDTO error = new ErrorDTO
            {
                Message = exception.Message,
                Source = Core.Constants.IntegrationPoints.APPLICATION_NAME,
                WorkspaceId = _WORKSPACE_ARTIFACT_ID
            };

            IErrorManager errorManager = Substitute.For<IErrorManager>();

            _integrationPointService
                .When(x => x.MarkIntegrationPointToStopJobs(_payload.AppId, _payload.ArtifactId))
                .Throw(exception);
            _managerFactory.CreateErrorManager().Returns(errorManager);
            errorManager.Create(Arg.Is<IEnumerable<ErrorDTO>>(x => x.First().Equals(error)));

            // Act
            HttpResponseMessage response = _instance.Stop(_payload);

            // Assert
            _integrationPointService
                .Received(1)
                .MarkIntegrationPointToStopJobs(_payload.AppId, _payload.ArtifactId);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "The HTTPStatusCode should be BadRequest");

            errorManager.Received(1).Create(Arg.Is<IEnumerable<ErrorDTO>>(x => x.First().Equals(error)));
        }

        private void AssertRDOsPermissions()
        {
            _permissionRepository.Received().UserHasArtifactTypePermissions(ObjectTypeGuids.IntegrationPointGuid, Arg.Any<IEnumerable<ArtifactPermission>>());
            _permissionRepository.Received().UserHasArtifactTypePermissions(ObjectTypeGuids.JobHistoryGuid, Arg.Any<IEnumerable<ArtifactPermission>>());
            _permissionRepository.Received().UserHasArtifactTypePermissions(ObjectTypeGuids.JobHistoryErrorGuid, Arg.Any<IEnumerable<ArtifactPermission>>());
            _permissionRepository.Received().UserHasArtifactTypePermission(ObjectTypeGuids.IntegrationPointTypeGuid, Arg.Any<ArtifactPermission>());
            _permissionRepository.Received().UserHasArtifactTypePermission(ObjectTypeGuids.SourceProviderGuid, Arg.Any<ArtifactPermission>());
            _permissionRepository.Received().UserHasArtifactTypePermission(ObjectTypeGuids.DestinationProviderGuid, Arg.Any<ArtifactPermission>());

        }
    }
}
