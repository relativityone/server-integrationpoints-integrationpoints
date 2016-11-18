using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Controllers.API;
using Newtonsoft.Json;
using NSubstitute;
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
		public override void SetUp()
		{
			_relativityUrlHelper = Substitute.For<IRelativityUrlHelper>();
			_integrationPointService = Substitute.For<IIntegrationPointService>();
			_rdoSynchronizerProvider = Substitute.For<IRdoSynchronizerProvider>();
			_cpHelper = Substitute.For<ICPHelper>();
			_svcMgr = Substitute.For<IServicesMgr>();

			_cpHelper.GetServicesManager().Returns(_svcMgr);
			_svcMgr.CreateProxy<IMetricsManager>(Arg.Any<ExecutionIdentity>()).Returns(Substitute.For<IMetricsManager>());

			_instance = new IntegrationPointsAPIController(_integrationPointService, _relativityUrlHelper,
				_rdoSynchronizerProvider, _cpHelper) {Request = new HttpRequestMessage()};
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
					_WORKSPACE_ID,
					model.ArtifactID,
					ObjectTypes.IntegrationPoint)
				.Returns(url);

			// Act
			HttpResponseMessage response = _instance.Update(_WORKSPACE_ID, model);

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
	}
}