using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Tests.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Unit.Controllers.API
{

	[TestFixture]
	public class IntegrationPointsAPIControllerTests : TestBase
	{
		private IntegrationPointsAPIController _instance;
		private IIntegrationPointService _integrationPointService;
		private ICaseServiceContext _caseServiceContext;
		private IPermissionService _permissionService;
		private IRdoSynchronizerProvider _rdoSynchronizerProvider;
		private IRelativityUrlHelper _relativityUrlHelper;

		private const int WORKSPACE_ID = 23432;

		[SetUp]
		public void TestFixtureSetUp()
		{
			_integrationPointService = this.GetMock<IIntegrationPointService>();
			_caseServiceContext = this.GetMock<ICaseServiceContext>();
			_permissionService = this.GetMock<IPermissionService>();
			_rdoSynchronizerProvider = this.GetMock<IRdoSynchronizerProvider>();
			_relativityUrlHelper = this.GetMock<IRelativityUrlHelper>();

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

			var existingModel = new IntegrationModel()
			{
				ArtifactID = model.ArtifactID,
				SourceProvider = model.SourceProvider
			};

			_integrationPointService.ReadIntegrationPoint(Arg.Is(model.ArtifactID))
				.Returns(existingModel);

			var sourceProvider = new SourceProvider()
			{
				Identifier	= "ID"
			};
			_caseServiceContext.RsapiService.SourceProviderLibrary
				.Read(Arg.Is(model.SourceProvider))
				.Returns(sourceProvider);

			_integrationPointService.SaveIntegration(Arg.Is(model)).Returns(model.ArtifactID);

			string url = "http://lolol.com";
			_relativityUrlHelper.GetRelativityViewUrl(
				Arg.Is(WORKSPACE_ID), 
				Arg.Is(model.ArtifactID),
				Arg.Is(Data.ObjectTypes.IntegrationPoint))
				.Returns(url);

			// Act
			HttpResponseMessage response = _instance.Update(WORKSPACE_ID, model);

			// Assert
			Assert.IsNotNull(response, "Response should not be null");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "HttpStatusCode should be OK");
		}
	}
}