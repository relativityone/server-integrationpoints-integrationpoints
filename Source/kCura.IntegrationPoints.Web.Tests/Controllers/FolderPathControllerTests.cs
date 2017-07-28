using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.Relativity.Client;
using kCura.Relativity.ImportAPI;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
	[TestFixture]
	public class FolderPathControllerTests : TestBase
	{
		private IRSAPIClient _client;
		private IImportApiFactory _importApiFactory;
		private IConfig _config;
		private IRSAPIService _rsapiService;
		private IChoiceService _choiceService;

		private HttpConfiguration _configuration;
		private FolderPathController _instance;

		[SetUp]
		public override void SetUp()
		{
			_client = Substitute.For<IRSAPIClient>();
			_importApiFactory = Substitute.For<IImportApiFactory>();
			_config = Substitute.For<IConfig>();
			_rsapiService = Substitute.For<IRSAPIService>();
			_choiceService = Substitute.For<IChoiceService>();
			_configuration = Substitute.For<HttpConfiguration>();

			HttpConfiguration config = new HttpConfiguration();
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			IHttpRoute route = config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}");
			HttpRouteData routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "GetFolderPathFieldsController" } });

			_instance = new FolderPathController(_client, _importApiFactory, _config, _choiceService, _rsapiService)
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
            GetFieldsSharedSetup();

            HttpResponseMessage response = _instance.GetFields();
			Assert.IsTrue(response.IsSuccessStatusCode);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

        [Test]
        public void GetLongTextFields_Success()
        {
            //ARRANGE
            GetFieldsSharedSetup();

            HttpResponseMessage response = _instance.GetLongTextFields();
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
		public void GetFields_Exception()
		{
			//ARRANGE
			string message = "This is an example failure";
			QueryResult result = new QueryResult
			{
				Success = false,
				Message = message
			};

			_client.Query(Arg.Any<APIOptions>(), Arg.Any<Query>())
				.Returns(result);
			
			// ACT/ASSERT
			Assert.That(() => _instance.GetFields(),
				Throws.Exception
					.TypeOf<Exception>()
					.With.Property("Message")
					.EqualTo(message));
		}

		[Test]
		public void GetFolderCount_UseFolderInformationPath_False()
		{
			// ARRANGE
			int integrationPointArtifactId = 1;
			int expectedFolderCount = 0;
			int workspaceId = 1234567;
			_client.APIOptions = new APIOptions(workspaceId);

			Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint
			{
				SourceConfiguration = "{SavedSearchArtifactId: 123}",
				DestinationConfiguration = "{UseFolderPathInformation: false}"
			};
			
			_rsapiService.IntegrationPointLibrary.Read(integrationPointArtifactId).Returns(integrationPoint);

			// ACT
			HttpResponseMessage response = _instance.GetFolderCount(integrationPointArtifactId);

			// ASSERT
			Assert.IsTrue(response.IsSuccessStatusCode);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			int folderCount = Int32.MinValue;
			response.TryGetContentValue(out folderCount);
			Assert.AreEqual(expectedFolderCount, folderCount);
		}

        private void GetFieldsSharedSetup()
        {
            string webServiceUrl = @"http://localhost/";
            int workspaceId = 123;
            int documentArtifactTypeId = 10;

            QueryResult result = new QueryResult { Success = true };
            ImportSettings settings = new ImportSettings { WebServiceURL = webServiceUrl };
            _client.APIOptions = new APIOptions(workspaceId);

            IImportAPI importApi = NSubstitute.Substitute.For<IExtendedImportAPI>();
			_choiceService.ConvertToFieldEntries(null).ReturnsForAnyArgs(new List<Contracts.Models.FieldEntry>());

            _config.WebApiPath
                .Returns(webServiceUrl);

            _importApiFactory.GetImportAPI(settings)
                .Returns(importApi);

            importApi.GetWorkspaceFields(workspaceId, documentArtifactTypeId);

            _client.Query(Arg.Any<APIOptions>(), Arg.Any<Query>())
                .Returns(result);
        }
	}
}
