using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using kCura.Relativity.Client;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Unit.Controllers
{
	[TestFixture]
	public class GetFolderPathFieldsControllerTests
	{
		private IRSAPIClient _client;
		private HttpConfiguration _configuration;
		private GetFolderPathFieldsController _instance;

		[SetUp]
		public void SetUp()
		{
			_client = NSubstitute.Substitute.For<IRSAPIClient>();
			_configuration = NSubstitute.Substitute.For<HttpConfiguration>();

			HttpConfiguration config = new HttpConfiguration();
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			IHttpRoute route = config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}");
			HttpRouteData routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "GetFolderPathFieldsController" } });

			_instance = new GetFolderPathFieldsController(_client)
			{
				ControllerContext = new HttpControllerContext(config, routeData, request),
				Request = request
			};
			_instance.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
			_instance.Configuration = _configuration;
		}

		[Test]
		public void Get_Success()
		{
			//ARRANGE
			QueryResult result = new QueryResult();
			result.Success = true;

			_client.Query(Arg.Any<APIOptions>(), Arg.Any<Query>())
				.Returns(result);

			HttpResponseMessage response = _instance.Get();
			Assert.IsTrue(response.IsSuccessStatusCode);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage = "This is an example failure")]
		public void Get_Exception()
		{
			//ARRANGE
			QueryResult result = new QueryResult();
			result.Success = false;
			result.Message = "This is an example failure";

			_client.Query(Arg.Any<APIOptions>(), Arg.Any<Query>())
				.Returns(result);

			_instance.Get();
		}
	}
}
