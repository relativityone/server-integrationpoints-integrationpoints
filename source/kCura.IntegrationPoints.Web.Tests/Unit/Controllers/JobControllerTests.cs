﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Unit.Controllers
{
	[TestFixture]
	public class JobControllerTests
	{
		private const int _WORKSPACE_ARTIFACT_ID = 1020530;
		private const int _INTEGRATION_POINT_ARTIFACT_ID = 1003663;
		private const int _USERID = 9;
		private readonly string _userIdString = _USERID.ToString();

		private JobController.Payload _payload;
		private IIntegrationPointService _integrationPointService;
		private ICPHelper _helper;
		private IContextContainerFactory _contextContainerFactory;
		private IManagerFactory _managerFactory;

		private JobController _instance;

		[SetUp]
		public void Setup()
		{
			_payload = new JobController.Payload { AppId = _WORKSPACE_ARTIFACT_ID, ArtifactId = _INTEGRATION_POINT_ARTIFACT_ID };
			
			_integrationPointService = Substitute.For<IIntegrationPointService>();
			_helper = Substitute.For<ICPHelper>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_managerFactory = Substitute.For<IManagerFactory>();

			_instance = new JobController(_integrationPointService, _helper, _contextContainerFactory, _managerFactory)
			{
				Request = new HttpRequestMessage()
			};
			_instance.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
		}

		[Test]
		public void ControllerDoesNotHaveUserIdInTheHeaderWhenTryingToSubmitPushingJob_ExpectBadRequest()
		{
			// Arrange
			const string expectedErrorMessage = @"Unable to determine the user id. Please contact your system administrator.";

			Exception exception = new Exception(expectedErrorMessage);
			_integrationPointService.When(
				service => service.RunIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, 0))
				.Throw(exception);

			// Act
			HttpResponseMessage response = _instance.Run(_payload);

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
			const string expectedErrorMessage = @"ABC : 123,456";

			AggregateException exceptionToBeThrown = new AggregateException("ABC",
				new[] { new AccessViolationException("123"), new Exception("456") });

			_integrationPointService.When(
				service => service.RunIntegrationPoint(_WORKSPACE_ARTIFACT_ID, _INTEGRATION_POINT_ARTIFACT_ID, _USERID))
				.Throw(exceptionToBeThrown);

			// Act
			HttpResponseMessage response = _instance.Run(_payload);

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
			HttpResponseMessage response = _instance.Run(_payload);

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
			ContextContainer contextContainer = new ContextContainer(_helper);
			IErrorManager errorManager = Substitute.For<IErrorManager>();

			_integrationPointService
				.When(x => x.MarkIntegrationPointToStopJobs(_payload.AppId, _payload.ArtifactId))
				.Throw(aggregateException);
			_contextContainerFactory.CreateContextContainer(_helper).Returns(contextContainer);
			_managerFactory.CreateErrorManager(contextContainer).Returns(errorManager);
			errorManager.Create(_WORKSPACE_ARTIFACT_ID, Arg.Is<IEnumerable<ErrorDTO>>(x => ValidateErrorDto(x.First(), expectedErrorMessage)));

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

			errorManager.Received(1).Create(_WORKSPACE_ARTIFACT_ID, Arg.Is<IEnumerable<ErrorDTO>>(x => ValidateErrorDto(x.First(), expectedErrorMessage)));
		}

		[Test]
		public void Stop_ExceptionThrown_ResponseIsCorrect()
		{
			// Arrange
			var exception = new Exception("exception message");
			ContextContainer contextContainer = new ContextContainer(_helper);
			IErrorManager errorManager = Substitute.For<IErrorManager>();

			_integrationPointService
				.When(x => x.MarkIntegrationPointToStopJobs(_payload.AppId, _payload.ArtifactId))
				.Throw(exception);
			_contextContainerFactory.CreateContextContainer(_helper).Returns(contextContainer);
			_managerFactory.CreateErrorManager(contextContainer).Returns(errorManager);
			errorManager.Create(_WORKSPACE_ARTIFACT_ID, Arg.Is<IEnumerable<ErrorDTO>>(x => ValidateErrorDto(x.First(), exception.Message)));

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

			errorManager.Received(1).Create(_WORKSPACE_ARTIFACT_ID, Arg.Is<IEnumerable<ErrorDTO>>(x => ValidateErrorDto(x.First(), exception.Message)));
		}

		private bool ValidateErrorDto(ErrorDTO error, string expectedMessage)
		{
			bool isCorrectMessage = error.Message == expectedMessage;
			bool isCorrectSource = error.Source == Core.Constants.IntegrationPoints.APPLICATION_NAME;
			return isCorrectMessage && isCorrectSource;
		}
	}
}