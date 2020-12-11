#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
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
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.Relativity.Client;
using kCura.Relativity.ImportAPI;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Field = kCura.Relativity.ImportAPI.Data.Field;
using Query = kCura.Relativity.Client.Query;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
	[TestFixture, Category("Unit")]
	public class FolderPathControllerTests : TestBase
	{
		private IRSAPIClient _client;
		private IFieldService _fieldService;
		private IImportApiFactory _importApiFactory;
		private IConfig _config;
		private IChoiceService _choiceService;
		private IWorkspaceContext _workspaceIdProvider;
		private IImportApiFacade _importApiFacade;
		private ICPHelper _helper;

		private HttpConfiguration _configuration;
		private FolderPathController _instance;

		[SetUp]
		public override void SetUp()
		{
			_client = Substitute.For<IRSAPIClient>();
			_fieldService = Substitute.For<IFieldService>();
			_importApiFactory = Substitute.For<IImportApiFactory>();
			_config = Substitute.For<IConfig>();
			_choiceService = Substitute.For<IChoiceService>();
			_workspaceIdProvider = Substitute.For<IWorkspaceContext>();
			_importApiFacade = Substitute.For<IImportApiFacade>();
			_configuration = Substitute.For<HttpConfiguration>();
			_helper = Substitute.For<ICPHelper>();

			var config = new HttpConfiguration();
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			IHttpRoute route = config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}");
			var routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "GetFolderPathFieldsController" } });

			_instance = new FolderPathController(_client, _fieldService, _choiceService, _workspaceIdProvider, _importApiFacade, _helper)
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
		public void GetChoiceFields_Success()
		{
			//ARRANGE
			GetFieldsSharedSetup();

			HttpResponseMessage response = _instance.GetChoiceFields();
			Assert.IsTrue(response.IsSuccessStatusCode);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		private void GetFieldsSharedSetup()
        {
            var webServiceUrl = @"http://localhost/";
            var workspaceId = 123;
            var documentArtifactTypeId = 10;

            var result = new QueryResult { Success = true };
            var settings = new ImportSettings { WebServiceURL = webServiceUrl };
			_client.APIOptions = new APIOptions(workspaceId);

			var listOfFields = new List<FieldEntry> {new FieldEntry()};
	        _choiceService.GetChoiceFields(Arg.Any<int>()).Returns(listOfFields);
	     
			IImportAPI importApi = Substitute.For<IImportAPI>();
			_choiceService.ConvertToFieldEntries(null).ReturnsForAnyArgs(new List<FieldEntry>());

			_config.WebApiPath.Returns(webServiceUrl);

	        _importApiFactory.GetImportAPI(settings).Returns(importApi);

			importApi.GetWorkspaceFields(workspaceId, documentArtifactTypeId).Returns(new List<Field> { new Field() });

			_client.Query(Arg.Any<APIOptions>(), Arg.Any<Query>()).Returns(result);

	        _workspaceIdProvider.GetWorkspaceID().Returns(workspaceId);

	        _fieldService.GetAllTextFields(Arg.Any<int>(), Arg.Any<int>()).Returns(new List<FieldEntry>());
	        _fieldService.GetLongTextFields(Arg.Any<int>(), Arg.Any<int>()).Returns(new List<FieldEntry>());
        }
		
	}
}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
