using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.Relativity.ImportAPI;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Unit.Controllers
{
	[TestFixture]
	public class FolderPathControllerTests
	{
		private IRSAPIClient _client;
		private IImportApiFactory _importApiFactory;
		private IConfig _config;
		private HttpConfiguration _configuration;
		private FolderPathController _instance;

		[SetUp]
		public void SetUp()
		{
			_client = NSubstitute.Substitute.For<IRSAPIClient>();
			_importApiFactory = NSubstitute.Substitute.For<IImportApiFactory>();
			_config = NSubstitute.Substitute.For<IConfig>();
			_configuration = NSubstitute.Substitute.For<HttpConfiguration>();

			HttpConfiguration config = new HttpConfiguration();
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			IHttpRoute route = config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}");
			HttpRouteData routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "GetFolderPathFieldsController" } });

			_instance = new FolderPathController(_client, _importApiFactory, _config)
			{
				ControllerContext = new HttpControllerContext(config, routeData, request),
				Request = request
			};
			_instance.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
			_instance.Configuration = _configuration;
		}

		[Test]
		public void GetFields_Success()
		{
			//ARRANGE
			string webServiceUrl = @"http://localhost/";
			int workspaceId = 123;
			int documentArtifactTypeId = 10;

			QueryResult result = new QueryResult { Success = true };
			ImportSettings settings = new ImportSettings { WebServiceURL = webServiceUrl };
			_client.APIOptions = new APIOptions(workspaceId);

			IImportAPI importApi = NSubstitute.Substitute.For<IImportAPI>();

			_config.WebApiPath
				.Returns(webServiceUrl);

			_importApiFactory.GetImportAPI(settings)
				.Returns(importApi);

			importApi.GetWorkspaceFields(workspaceId, documentArtifactTypeId);

			_client.Query(Arg.Any<APIOptions>(), Arg.Any<Query>())
				.Returns(result);

			HttpResponseMessage response = _instance.GetFields();
			Assert.IsTrue(response.IsSuccessStatusCode);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage = "This is an example failure")]
		public void GetFields_Exception()
		{
			//ARRANGE
			QueryResult result = new QueryResult
			{
				Success = false,
				Message = "This is an example failure"
			};

			_client.Query(Arg.Any<APIOptions>(), Arg.Any<Query>())
				.Returns(result);

			_instance.GetFields();
		}
	}
}
