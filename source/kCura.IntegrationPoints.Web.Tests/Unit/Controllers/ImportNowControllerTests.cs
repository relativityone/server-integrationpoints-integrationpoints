using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Unit.Controllers
{
	[TestFixture]
	public class ImportNowControllerTests
	{
		private ImportNowController _controller;
		private ImportNowController.Payload _payload;
		private readonly string _userIdString = _USERID.ToString();
		private ICaseServiceContext _caseSericeContext;
		private IIntegrationPointService _integrationPointService;
		private const int _USERID = 9;

		[SetUp]
		public void Setup()
		{
			_payload = new ImportNowController.Payload()
			{
				AppId = 1,
				ArtifactId = 123
			};

			_caseSericeContext = Substitute.For<ICaseServiceContext>();
			_integrationPointService = Substitute.For<IIntegrationPointService>();

			_controller = new ImportNowController(_caseSericeContext, _integrationPointService);
			_controller.Request = new HttpRequestMessage();
			_controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
		}

		[Test]
		public void ControllerDoesNotHaveUserIdInTheHeaderWhenTryingToSubmitPushingJob_ExpectBadRequest()
		{
			const string expectedErrorMessage = @"""Unable to determine the user id. Please contact your system administrator.""";

			Exception exception = new Exception("Unable to determine the user id. Please contact your system administrator.");
			_integrationPointService.When(service => service.RunIntegrationPoint(1, 123, 0)).Throw(exception);

			HttpResponseMessage response = _controller.Post(_payload);

			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.AreEqual(expectedErrorMessage, response.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void RsapiCallThrowsException()
		{
			List<Claim> claims = new List<Claim>()
			{
				new Claim("rel_uai", _userIdString)
			};
			_controller.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
			const string expectedErrorMessage = @"""ABC : 123,456""";

			AggregateException exceptionToBeThrown = new AggregateException("ABC",
				new[] { new AccessViolationException("123"), new Exception("456") });

			_integrationPointService.When(service => service.RunIntegrationPoint(1, 123, _USERID)).Throw(exceptionToBeThrown);

			HttpResponseMessage response = _controller.Post(_payload);

			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.AreEqual(expectedErrorMessage, response.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void ControllerDoesNotHaveUserIdInTheHeaderWhenTryingToSubmitNormalJob_ExpectNoError()
		{
			List<Claim> claims = new List<Claim>();
			_controller.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

			HttpResponseMessage response = _controller.Post(_payload);


			_integrationPointService.Received(1).RunIntegrationPoint(1, 123, 0);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void NonRelativityProviderCall()
		{
			List<Claim> claims = new List<Claim>()
			{
				new Claim("rel_uai", _userIdString)
			};
			_controller.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

			HttpResponseMessage response = _controller.Post(_payload);
			_integrationPointService.Received(1).RunIntegrationPoint(1, 123, _USERID);

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}
	}
}