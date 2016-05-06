﻿using System;
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
		private const int _WORKSPACE_ARTIFACT_ID = 1020530;
		private const int _INTEGRATION_POINT_ARTIFACT_ID = 1003663;
		private const int _USERID = 9;
		private readonly string _userIdString = _USERID.ToString();

		private ImportNowController.Payload _payload;
		private ICaseServiceContext _caseSericeContext;
		private IIntegrationPointService _integrationPointService;

		private ImportNowController _instance;

		[SetUp]
		public void Setup()
		{
			_payload = new ImportNowController.Payload { AppId = _WORKSPACE_ARTIFACT_ID, ArtifactId = _INTEGRATION_POINT_ARTIFACT_ID };

			_caseSericeContext = Substitute.For<ICaseServiceContext>();
			_integrationPointService = Substitute.For<IIntegrationPointService>();

			_instance = new ImportNowController(_caseSericeContext, _integrationPointService)
			{
				Request = new HttpRequestMessage()
			};
			_instance.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
		}

		[Test]
		public void ControllerDoesNotHaveUserIdInTheHeaderWhenTryingToSubmitPushingJob_ExpectBadRequest()
		{
			// Arrange
			const string expectedErrorMessage = @"""Unable to determine the user id. Please contact your system administrator.""";

			Exception exception = new Exception("Unable to determine the user id. Please contact your system administrator.");
			_integrationPointService.When(
				service => service.RunIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, 0))
				.Throw(exception);

			// Act
			HttpResponseMessage response = _instance.Post(_payload);

			// Assert
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.AreEqual(expectedErrorMessage, response.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void RsapiCallThrowsException()
		{
			// Arrange
			var claims = new List<Claim>(1)
			{
				new Claim("rel_uai", _userIdString)
			};
			_instance.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
			const string expectedErrorMessage = @"""ABC : 123,456""";

			AggregateException exceptionToBeThrown = new AggregateException("ABC",
				new[] { new AccessViolationException("123"), new Exception("456") });

			_integrationPointService.When(
				service => service.RunIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, _USERID))
				.Throw(exceptionToBeThrown);

			// Act
			HttpResponseMessage response = _instance.Post(_payload);

			// Assert
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.AreEqual(expectedErrorMessage, response.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void ControllerDoesNotHaveUserIdInTheHeaderWhenTryingToSubmitNormalJob_ExpectNoError()
		{
			// Arrange
			_instance.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(0)));

			// Act
			HttpResponseMessage response = _instance.Post(_payload);

			// Assert
			_integrationPointService.Received(1).RunIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, 0);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void NonRelativityProviderCall()
		{
			// Arrange
			var claims = new List<Claim>(1)
			{
				new Claim("rel_uai", _userIdString)
			};
			_instance.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

			// Act
			HttpResponseMessage response = _instance.Post(_payload);

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
			HttpResponseMessage response = _instance.RetryJob(_payload);

			// Assert
			_integrationPointService.Received(1).RetryIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, _USERID);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void RetryJob_UserIdDoesNotExist_Succeeds_Test()
		{
			// Arrange
			_instance.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(0)));

			// Act
			HttpResponseMessage response = _instance.RetryJob(_payload);

			// Assert
			_integrationPointService.Received(1).RetryIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, 0);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}
	}
}