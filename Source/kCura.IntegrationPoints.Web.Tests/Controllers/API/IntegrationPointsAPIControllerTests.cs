using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Tests.Helpers;
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
		private IntegrationPointsAPIController _instance;
		private IIntegrationPointService _integrationPointService;
		private IRelativityUrlHelper _relativityUrlHelper;
		private IRdoSynchronizerProvider _rdoSynchronizerProvider;
		private ICPHelper _cpHelper;
		private IServicesMgr _svcMgr;

		private const int _WORKSPACE_ID = 23432;

		[SetUp]
		public new void OneTimeSetUp()
		{
			_relativityUrlHelper = this.GetMock<IRelativityUrlHelper>();
			_integrationPointService = this.GetMock<IIntegrationPointService>();
			_rdoSynchronizerProvider = this.GetMock<IRdoSynchronizerProvider>();
			_cpHelper = this.GetMock<ICPHelper>();
			_svcMgr = Substitute.For<IServicesMgr>();

			_cpHelper.GetServicesManager().Returns(_svcMgr);
			_svcMgr.CreateProxy<IMetricsManager>(Arg.Any<ExecutionIdentity>()).Returns(Substitute.For<IMetricsManager>());

			_instance = this.ResolveInstance<IntegrationPointsAPIController>();
			_instance.Request = new HttpRequestMessage();
			_instance.Request.SetConfiguration(new HttpConfiguration());
		}

		[Test]
		public void Update_StandardSourceProvider_NoJobsRun_GoldFlow()
		{
			// Arrange
			var model = new IntegrationModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830
			};

			_integrationPointService.SaveIntegration(Arg.Is(model)).Returns(model.ArtifactID);

			string url = "http://lolol.com";
			_relativityUrlHelper.GetRelativityViewUrl(
				Arg.Is(_WORKSPACE_ID),
				Arg.Is(model.ArtifactID),
				Arg.Is(Data.ObjectTypes.IntegrationPoint))
				.Returns(url);

			// Act
			HttpResponseMessage response = _instance.Update(_WORKSPACE_ID, model);

			// Assert
			Assert.IsNotNull(response, "Response should not be null");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "HttpStatusCode should be OK");
			Assert.AreEqual(JsonConvert.SerializeObject(new { returnURL = url }), response.Content.ReadAsStringAsync().Result, "The HttpContent should be as expected");

			_integrationPointService.Received(1).SaveIntegration(Arg.Is(model));
			_relativityUrlHelper
				.Received(1)
				.GetRelativityViewUrl(
					Arg.Is(_WORKSPACE_ID),
					Arg.Is(model.ArtifactID),
					Arg.Is(Data.ObjectTypes.IntegrationPoint));
		}

		[Test]
		public void UpdateIntegrationPointThrowsError_ReturnFailedResponse()
		{
			const string expect = "That's a damn shame.";
			Exception expectException = new Exception(expect);
			_integrationPointService.SaveIntegration(Arg.Any<IntegrationModel>()).Throws(expectException);

			// Act
			HttpResponseMessage response = _instance.Update(_WORKSPACE_ID, new IntegrationModel());

			Assert.IsNotNull(response);
			String actual = response.Content.ReadAsStringAsync().Result;
			_relativityUrlHelper.DidNotReceive().GetRelativityViewUrl(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
			Assert.AreEqual($"\"{expect}\"", actual);
			Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
		}

		[Test]
		public void GetRelativityViewUrlThrowsError_ReturnFailedResponse()
		{
			const string expect = "That's a damn shame.";
			Exception expectException = new Exception(expect);

			_integrationPointService.SaveIntegration(Arg.Any<IntegrationModel>()).Returns(3);
			_relativityUrlHelper.GetRelativityViewUrl(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>()).Throws(expectException);

			// Act
			HttpResponseMessage response = _instance.Update(_WORKSPACE_ID, new IntegrationModel());

			Assert.IsNotNull(response);
			String actual = response.Content.ReadAsStringAsync().Result;
			Assert.AreEqual($"\"{expect}\"", actual);
			Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
		}
	}
}