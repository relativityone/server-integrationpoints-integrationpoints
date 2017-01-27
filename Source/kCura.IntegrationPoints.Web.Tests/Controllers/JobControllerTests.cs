using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers.API;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
	[TestFixture]
	public class JobControllerTests : TestBase
	{
		private const string _RUN_AUDIT_MESSAGE = "Transfer was attempted.";
		private const string _RETRY_AUDIT_MESSAGE = "Retry error was attempted.";
		private const string _STOP_AUDIT_MESSAGE = "Stop transfer was attempted.";

		private const int _WORKSPACE_ARTIFACT_ID = 1020530;
		private const int _INTEGRATION_POINT_ARTIFACT_ID = 1003663;
		private const int _USERID = 9;
		private readonly string _userIdString = _USERID.ToString();

		private JobController.Payload _payload;
		private IIntegrationPointService _integrationPointService;
		private ICPHelper _helper;
		private IHelper _targetHelper;
		private IContextContainerFactory _contextContainerFactory;
		private IHelperFactory _helperFactory;
		private IServiceFactory _serviceFactory;
		private ICaseServiceContext _caseServiceContext;
		private ISerializer _serializer;
		private IChoiceQuery _choiceQuery;
		private IJobManager _jobManager;
		private IManagerFactory _managerFactory;
		private IRepositoryFactory _repositoryFactory;
		private IIntegrationPointRepository _integrationPointRepository;
		private IContextContainer _contextContainer;
		private IJobHistoryService _jobHistoryService;
		private IIntegrationPointProviderValidator _ipValidator;
		private IIntegrationPointPermissionValidator _permissionValidator;
		private JobController _instance;
		private IRelativityAuditRepository _auditRepository;
		private IAuditManager _auditManager;
		private IToggleProvider _toggleProvider;

		[SetUp]
		public override void SetUp()
		{
			_payload = new JobController.Payload { AppId = _WORKSPACE_ARTIFACT_ID, ArtifactId = _INTEGRATION_POINT_ARTIFACT_ID };
			
			_integrationPointService = Substitute.For<IIntegrationPointService>();
			_helper = Substitute.For<ICPHelper>();
			_targetHelper = Substitute.For<IHelper>();
			_helperFactory = Substitute.For<IHelperFactory>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_auditManager = Substitute.For<IAuditManager>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
			_auditRepository = Substitute.For<IRelativityAuditRepository>();
			_serviceFactory = Substitute.For<IServiceFactory>();
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_serializer = Substitute.For<ISerializer>();
			_choiceQuery = Substitute.For<IChoiceQuery>();
			_jobManager = Substitute.For<IJobManager>();
			_contextContainer = Substitute.For<IContextContainer>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_ipValidator = Substitute.For<IIntegrationPointProviderValidator>();
			_permissionValidator = Substitute.For<IIntegrationPointPermissionValidator>();
			_toggleProvider = Substitute.For<IToggleProvider>();

			_helper.GetActiveCaseID().Returns(_WORKSPACE_ARTIFACT_ID);
			_helperFactory.CreateTargetHelper(_helper).Returns(_helper);
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_managerFactory.CreateJobHistoryService(_caseServiceContext, _contextContainer, _serializer).Returns(_jobHistoryService);
			_serviceFactory.CreateIntegrationPointService(_helper, _helper, _caseServiceContext, _contextContainerFactory, _serializer, _choiceQuery, _jobManager, _managerFactory, _ipValidator, _permissionValidator, _toggleProvider).Returns(_integrationPointService);
			_serviceFactory.CreateIntegrationPointService(_helper, _targetHelper, _caseServiceContext, _contextContainerFactory, _serializer, _choiceQuery, _jobManager, _managerFactory, _ipValidator, _permissionValidator, _toggleProvider).Returns(_integrationPointService);
			_managerFactory.CreateAuditManager(_contextContainer, _WORKSPACE_ARTIFACT_ID).Returns(_auditManager);
			_repositoryFactory.GetIntegrationPointRepository(_WORKSPACE_ARTIFACT_ID).Returns(_integrationPointRepository);
			_auditManager.RelativityAuditRepository.Returns(_auditRepository);

			_instance = new JobController(
				_serviceFactory, 
				_helper,
				_helperFactory,
				_caseServiceContext,
				_contextContainerFactory,
				_serializer,
				_choiceQuery,
				_jobManager,
				_managerFactory,
				_repositoryFactory,
				_ipValidator,
				_permissionValidator,
				_toggleProvider)
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
			var integrationPoint = new IntegrationPointDTO()
			{
				DestinationConfiguration = JsonConvert.SerializeObject(new ImportSettings() { FederatedInstanceArtifactId = federatedInstanceArtifactId })
			};
			const string expectedErrorMessage = @"Unable to determine the user id. Please contact your system administrator.";

			Exception exception = new Exception(expectedErrorMessage);

			_helperFactory.CreateTargetHelper(_helper, federatedInstanceArtifactId).Returns(_targetHelper);

			_integrationPointRepository.Read(_INTEGRATION_POINT_ARTIFACT_ID).Returns(integrationPoint);

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
			Assert.AreEqual(expectedErrorMessage, response.Content.ReadAsStringAsync().Result);
		}

		[TestCase(null)]
		[TestCase(1000)]
		public void RsapiCallThrowsException(int? federatedInstanceArtifactId)
		{
			// Arrange
			var claims = new List<Claim>(1)
			{
				new Claim("rel_uai", _userIdString)
			};
			_instance.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
			const string expectedErrorMessage = @"ABC : 123,456";

			AggregateException exceptionToBeThrown = new AggregateException("ABC",
				new[] { new AccessViolationException("123"), new Exception("456") });

			var integrationPoint = new IntegrationPointDTO()
			{
				DestinationConfiguration = JsonConvert.SerializeObject(new ImportSettings() { FederatedInstanceArtifactId = federatedInstanceArtifactId })
			};

			_helperFactory.CreateTargetHelper(_helper, federatedInstanceArtifactId).Returns(_targetHelper);

			_integrationPointRepository.Read(_INTEGRATION_POINT_ARTIFACT_ID).Returns(integrationPoint);

			_integrationPointService.When(
				service => service.RunIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, _USERID))
				.Throw(exceptionToBeThrown);

			// Act
			HttpResponseMessage response = _instance.Run(_payload);

			// Assert
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.AreEqual(expectedErrorMessage, response.Content.ReadAsStringAsync().Result);
		}

		[TestCase(null)]
		[TestCase(1000)]
		public void ControllerDoesNotHaveUserIdInTheHeaderWhenTryingToSubmitNormalJob_ExpectNoError(int? federatedInstanceArtifactId)
		{
			// Arrange
			var integrationPoint = new IntegrationPointDTO()
			{
				DestinationConfiguration = JsonConvert.SerializeObject(new ImportSettings() { FederatedInstanceArtifactId = federatedInstanceArtifactId })
			};

			_helperFactory.CreateTargetHelper(_helper, federatedInstanceArtifactId).Returns(_targetHelper);
			_integrationPointRepository.Read(_INTEGRATION_POINT_ARTIFACT_ID).Returns(integrationPoint);
			_instance.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(0)));

			// Act
			HttpResponseMessage response = _instance.Run(_payload);

			// Assert
			_integrationPointService.Received(1).RunIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, 0);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
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

			var integrationPoint = new IntegrationPointDTO()
			{
				DestinationConfiguration = JsonConvert.SerializeObject(new ImportSettings() { FederatedInstanceArtifactId = federatedInstanceArtifactId })
			};

			_helperFactory.CreateTargetHelper(_helper, federatedInstanceArtifactId).Returns(_targetHelper);
			_integrationPointRepository.Read(_INTEGRATION_POINT_ARTIFACT_ID).Returns(integrationPoint);
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
			_instance.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

			// Act
			HttpResponseMessage response = _instance.Retry(_payload);

			// Assert
			_auditRepository.Received(1)
							.CreateAuditRecord(_payload.ArtifactId,
							Arg.Is<AuditElement>(audit => audit.AuditMessage == _RETRY_AUDIT_MESSAGE));
			_integrationPointService.Received(1).RetryIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, _USERID);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void RetryJob_UserIdDoesNotExist_IntegrationPointServiceThrowsError_Test()
		{
			// Arrange
			_instance.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(0)));
			var exception = new Exception(Core.Constants.IntegrationPoints.NO_USERID);
			_integrationPointService.When(x => x.RetryIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, 0))
				.Throw(exception);

			// Act
			HttpResponseMessage response = _instance.Retry(_payload);

			// Assert
			_auditRepository.Received(1)
				.CreateAuditRecord(_payload.ArtifactId,
				Arg.Is<AuditElement>(audit => audit.AuditMessage == _RETRY_AUDIT_MESSAGE));
			_integrationPointService.Received(1).RetryIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, 0);
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.AreEqual(Core.Constants.IntegrationPoints.NO_USERID, response.Content.ReadAsStringAsync().Result.Trim('"'));
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
			Assert.IsNull(response.Content, "The response's Content should be null");
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
			_managerFactory.CreateErrorManager(_contextContainer).Returns(errorManager);
			errorManager.Create(Arg.Is<IEnumerable<ErrorDTO>>(x => x.First().Equals(error)));

			// Act
			HttpResponseMessage response = _instance.Stop(_payload);

			// Assert
			_integrationPointService
				.Received(1)
				.MarkIntegrationPointToStopJobs(_payload.AppId, _payload.ArtifactId);

			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "The HTTPStatusCode should be BadRequest");

			byte[] utf8Bytes = response.Content.ReadAsByteArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			string stringContent = System.Text.Encoding.UTF8.GetString(utf8Bytes);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType, "The response's media type should be correct.");
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet, "The response's char set should be correct.");
			Assert.AreEqual(expectedErrorMessage, stringContent, "The response's Content should be correct.");

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
			_managerFactory.CreateErrorManager(_contextContainer).Returns(errorManager);
			errorManager.Create(Arg.Is<IEnumerable<ErrorDTO>>(x => x.First().Equals(error)));

			// Act
			HttpResponseMessage response = _instance.Stop(_payload);

			// Assert
			_integrationPointService
				.Received(1)
				.MarkIntegrationPointToStopJobs(_payload.AppId, _payload.ArtifactId);

			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "The HTTPStatusCode should be BadRequest");
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType, "The response's media type should be correct.");
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet, "The response's char set should be correct.");

			byte[] utf8Bytes = response.Content.ReadAsByteArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			string stringContent = System.Text.Encoding.UTF8.GetString(utf8Bytes);
			Assert.AreEqual(exception.Message, stringContent, "The response's Content should be correct.");

			errorManager.Received(1).Create(Arg.Is<IEnumerable<ErrorDTO>>(x => x.First().Equals(error)));
		}
	}
}