﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.IntegrationPoints.Web.Models.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	[TestFixture]
	public class IntegrationPointsAPIControllerTests : TestBase
	{
		private IntegrationPointsAPIController _sut;
		private IServiceFactory _serviceFactory;
		private IIntegrationPointService _integrationPointService;
		private IRelativityUrlHelper _relativityUrlHelper;
		private IRdoSynchronizerProvider _rdoSynchronizerProvider;
		private ICPHelper _cpHelper;
		private IServicesMgr _svcMgr;

		private const int _WORKSPACE_ID = 23432;
		private const int _INTEGRATION_POINT_ID = 23432;
		private const string _CREDENTIALS = "{}";

		[SetUp]
		public override void SetUp()
		{
			_relativityUrlHelper = Substitute.For<IRelativityUrlHelper>();
			_integrationPointService = Substitute.For<IIntegrationPointService>();
			_rdoSynchronizerProvider = Substitute.For<IRdoSynchronizerProvider>();
			_serviceFactory = Substitute.For<IServiceFactory>();
			_cpHelper = Substitute.For<ICPHelper>();
			_svcMgr = Substitute.For<IServicesMgr>();

			_cpHelper.GetServicesManager().Returns(_svcMgr);
			_svcMgr.CreateProxy<IMetricsManager>(Arg.Any<ExecutionIdentity>())
				.Returns(Substitute.For<IMetricsManager>());

			_sut = new IntegrationPointsAPIController(
				_serviceFactory,
				_relativityUrlHelper,
				_rdoSynchronizerProvider,
				_cpHelper)
			{
				Request = new HttpRequestMessage()
			};

			_sut.Request.SetConfiguration(new HttpConfiguration());
		}


		[Test]
		public async Task Get_WhenFederatedInstanceIsSetUp_ShouldReturnNullSourceConfiguration()
		{
			// Arrange
			var model = new IntegrationPointModel()
			{
				ArtifactID = 123,
				SourceConfiguration = JsonConvert.SerializeObject(new ImportSettings() { FederatedInstanceArtifactId = 12345 })
			};

			_serviceFactory.CreateIntegrationPointService(_cpHelper).Returns(_integrationPointService);

			_integrationPointService.ReadIntegrationPointModel(Arg.Any<int>()).Returns(model);

			// Act
			HttpResponseMessage httpResponse = _sut.Get(_INTEGRATION_POINT_ID);

			// Assert
			IntegrationPointModel integrationPointModel = await GetIntegrationPointModelFromHttpResponse(httpResponse).ConfigureAwait(false);
			integrationPointModel.SourceConfiguration.Should().BeNull();
		}

		[Test]
		public async Task Get_WhenFederatedInstanceIsNotSetUp_ShouldReturnValidSourceConfiguration()
		{
			// Arrange
			var model = new IntegrationPointModel()
			{
				ArtifactID = 123,
				SourceConfiguration = JsonConvert.SerializeObject(new ImportSettings() { FederatedInstanceArtifactId = null })
			};

			_serviceFactory.CreateIntegrationPointService(_cpHelper).Returns(_integrationPointService);

			_integrationPointService.ReadIntegrationPointModel(Arg.Any<int>()).Returns(model);

			// Act
			HttpResponseMessage httpResponse = _sut.Get(_INTEGRATION_POINT_ID);

			// Assert
			IntegrationPointModel integrationPointModel = await GetIntegrationPointModelFromHttpResponse(httpResponse).ConfigureAwait(false);
			integrationPointModel.SourceConfiguration.Should().NotContain("FederatedInstanceArtifactId");
		}

		[TestCase(null)]
		[TestCase(1000)]
		public void Update_StandardSourceProvider_NoJobsRun_GoldFlow(int? federatedInstanceArtifactId)
		{
			// Arrange
			var model = new IntegrationPointModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				Destination = JsonConvert.SerializeObject(new ImportSettings() { FederatedInstanceArtifactId = federatedInstanceArtifactId }),
				SecuredConfiguration = _CREDENTIALS
			};

			_serviceFactory.CreateIntegrationPointService(_cpHelper).Returns(_integrationPointService);

			_integrationPointService.SaveIntegration(Arg.Is(model)).Returns(model.ArtifactID);

			string url = "http://lolol.com";
			_relativityUrlHelper.GetRelativityViewUrl(
					_WORKSPACE_ID,
					model.ArtifactID,
					ObjectTypes.IntegrationPoint)
				.Returns(url);

			// Act
			HttpResponseMessage response = _sut.Update(_WORKSPACE_ID, model);

			// Assert
			Assert.IsNotNull(response, "Response should not be null");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "HttpStatusCode should be OK");
			Assert.AreEqual(JsonConvert.SerializeObject(new { returnURL = url }), response.Content.ReadAsStringAsync().Result, "The HttpContent should be as expected");

			_integrationPointService.Received(1).SaveIntegration(model);
			_relativityUrlHelper
				.Received(1)
				.GetRelativityViewUrl(
					_WORKSPACE_ID,
					model.ArtifactID,
					ObjectTypes.IntegrationPoint);
		}

		[TestCase(null)]
		[TestCase(1000)]
		public void UpdateIntegrationPointThrowsError_ReturnFailedResponse(int? federatedInstanceArtifactId)
		{
			var model = new IntegrationPointModel()
			{
				Destination = JsonConvert.SerializeObject(new ImportSettings() { FederatedInstanceArtifactId = federatedInstanceArtifactId }),
				SecuredConfiguration = _CREDENTIALS
			};
			var validationResult = new ValidationResult(false, "That's a damn shame.");
			Exception expectException = new IntegrationPointValidationException(validationResult);

			_serviceFactory.CreateIntegrationPointService(_cpHelper).Returns(_integrationPointService);

			_integrationPointService.SaveIntegration(Arg.Any<IntegrationPointModel>()).Throws(expectException);

			// Act
			HttpResponseMessage response = _sut.Update(_WORKSPACE_ID, model);

			Assert.IsNotNull(response);
			String actual = response.Content.ReadAsStringAsync().Result;
			_relativityUrlHelper.DidNotReceive().GetRelativityViewUrl(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
			ValidationResultDTO actualResult =
				JsonConvert.DeserializeObject<ValidationResultDTO>(actual);

			Assert.AreEqual(validationResult.MessageTexts.First(), actualResult.Errors.Single().Message);
			Assert.AreEqual(HttpStatusCode.NotAcceptable, response.StatusCode);
		}

		private async Task<IntegrationPointModel> GetIntegrationPointModelFromHttpResponse(HttpResponseMessage httpResponse)
		{
			string serializedModel = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
			return JsonConvert.DeserializeObject<IntegrationPointModel>(serializedModel);
		}
	}
}