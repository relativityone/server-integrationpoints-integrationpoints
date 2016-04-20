using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Unit.Controllers
{
	[TestFixture]
	public class ImportNowControllerTests
	{
		private ImportNowController _controller;
		private IJobManager _jobManager;
		private IPermissionService _permissionService;
		private ImportNowController.IIntegrationPointRdoAdaptor _rdoAdaptor;
		private ImportNowController.Payload _payload;
		private readonly string USERID_STRING = USERID.ToString();
		private const int USERID = 9;

		[SetUp]
		public void Setup()
		{
			_payload = new ImportNowController.Payload()
			{
				AppId = 1,
				ArtifactId = 123
			};

			_jobManager = Substitute.For<IJobManager>();
			_permissionService = Substitute.For<IPermissionService>();
			_rdoAdaptor = Substitute.For<ImportNowController.IIntegrationPointRdoAdaptor>();
			_controller = new ImportNowController(_jobManager, _permissionService, _rdoAdaptor);
			_controller.Request = new HttpRequestMessage();
			_controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
		}

		[Test]
		public void UserDoesNotHavePermissionToPushToTheDestinationWorkspace()
		{
			List<Claim> claims = new List<Claim>()
			{
				new Claim("rel_uai", USERID_STRING)
			};
			_controller.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
			const string expectedErrorMessage = @"""You do not have permission to push documents to the destination workspace selected. Please contact your system administrator.""";

			_rdoAdaptor.SourceProviderIdentifier.Returns(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID);
			_rdoAdaptor.SourceConfiguration.Returns("{TargetWorkspaceArtifactId : 123}");
			_permissionService.UserCanImport(123).Returns(false);

			HttpResponseMessage response = _controller.Post(_payload);

			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.AreEqual(expectedErrorMessage, response.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void UserDoesHaveAPermissionToPushToAnotherWorkspace()
		{
			List<Claim> claims = new List<Claim>()
			{
				new Claim("rel_uai", USERID_STRING)
			};
			_controller.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
			_rdoAdaptor.SourceProviderIdentifier.Returns(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID);
			_rdoAdaptor.SourceConfiguration.Returns("{TargetWorkspaceArtifactId : 123}");
			_permissionService.UserCanImport(123).Returns(true);
			
			HttpResponseMessage response = _controller.Post(_payload);

			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.ExportService, _payload.AppId, _payload.ArtifactId, USERID);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void	ControllerDoesNotHaveUserIdInTheHeaderWhenTryingToSubmitPushingJob_ExpectBadRequest()
		{
			const string expectedErrorMessage = @"""Unable to determine the user id. Please contact your system administrator.""";
			List<Claim> claims = new List<Claim>();
			_controller.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
			_rdoAdaptor.SourceProviderIdentifier.Returns(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID);
			_rdoAdaptor.SourceConfiguration.Returns("{TargetWorkspaceArtifactId : 123}");
			_permissionService.UserCanImport(123).Returns(true);

			HttpResponseMessage response = _controller.Post(_payload);

			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.AreEqual(expectedErrorMessage, response.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void RsapiCallThrowsException()
		{
			List<Claim> claims = new List<Claim>()
			{
				new Claim("rel_uai", USERID_STRING)
			};
			_controller.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
			const string expectedErrorMessage = @"""ABC : 123,456""";

			AggregateException exceptionToBeThrown = new AggregateException("ABC",
				new[] { new AccessViolationException("123"), new Exception("456") });

			_rdoAdaptor.SourceProviderIdentifier.Throws(exceptionToBeThrown);
			HttpResponseMessage response = _controller.Post(_payload);

			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.AreEqual(expectedErrorMessage, response.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void ControllerDoesNotHaveUserIdInTheHeaderWhenTryingToSubmitNormalJob_ExpectNoError()
		{
			List<Claim> claims = new List<Claim>();
			_controller.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
			_rdoAdaptor.SourceProviderIdentifier.Returns(Guid.NewGuid().ToString());

			HttpResponseMessage response = _controller.Post(_payload);

			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.SyncManager, _payload.AppId, _payload.ArtifactId, 0);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}


		[Test]
		public void NonRelativityProviderCall()
		{
			List<Claim> claims = new List<Claim>()
			{
				new Claim("rel_uai", USERID_STRING)
			};
			_controller.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
			_rdoAdaptor.SourceProviderIdentifier.Returns(Guid.NewGuid().ToString());

			HttpResponseMessage response = _controller.Post(_payload);

			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.SyncManager, _payload.AppId, _payload.ArtifactId, USERID);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}
	}
}