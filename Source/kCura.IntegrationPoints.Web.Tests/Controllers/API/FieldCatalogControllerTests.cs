using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Controllers.API;
using Relativity.Services.FieldMapping;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	public class FieldCatalogControllerTests
	{
		private FieldCatalogController _controller;
		private IFieldCatalogService _service;
		private ExternalMapping[] _externalMappingArray;
		private HttpConfiguration _configuration;
		private ICPHelper _helper;
		private IHelperFactory _helperFactory;
		private IServiceFactory _serviceFactory;
		private const int ARRAY_SIZE = 5;

		[SetUp]
		public void SetUp()
		{
			_service = Substitute.For<IFieldCatalogService>();
			_configuration = Substitute.For<HttpConfiguration>();
			_helper = Substitute.For<ICPHelper>();
			_helperFactory = Substitute.For<IHelperFactory>();
			_serviceFactory = Substitute.For<IServiceFactory>();

			_helperFactory.CreateTargetHelper(_helper).Returns(_helper);
			_serviceFactory.CreateFieldCatalogService(_helper).Returns(_service);

			HttpConfiguration config = new HttpConfiguration();
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			IHttpRoute route = config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}");
			HttpRouteData routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "GetFieldCatalogController" } });

			_controller = new FieldCatalogController(_helper, _helperFactory, _serviceFactory)
			{
				ControllerContext = new HttpControllerContext(config, routeData, request),
				Request = request
			};
			_controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
			_controller.Configuration = _configuration;

			CreateExternalMapping();
		}

		[Test]
		public void ItShouldReturnFieldCatalogMappings()
		{
			_service.GetAllFieldCatalogMappings(123456).ReturnsForAnyArgs(_externalMappingArray);

			HttpResponseMessage responseMessage = _controller.Get(123456);
			ExternalMapping[] result = ExtractMappingResponse(responseMessage);

			//assert that the starting array and result of the controller call are equal
			CollectionAssert.AreEqual(_externalMappingArray, result);
		}

		private ExternalMapping[] ExtractMappingResponse(HttpResponseMessage response)
		{

			ObjectContent objectContent = response.Content as ObjectContent;
			ExternalMapping[] result = (ExternalMapping[])objectContent?.Value;
			return result;
		}

		private void CreateExternalMapping()
		{
			_externalMappingArray = new ExternalMapping[ARRAY_SIZE];
			ExternalMapping map;
			for (int i = 0; i < ARRAY_SIZE; i++)
			{
				map = new ExternalMapping();
				map.FriendlyName = string.Format("FriendlyName{0}", i);
				map.FieldArtifactId = i;
				map.ExternalFieldSource = "Invariant";
				_externalMappingArray[i] = map;
			}
		}
	}
}
