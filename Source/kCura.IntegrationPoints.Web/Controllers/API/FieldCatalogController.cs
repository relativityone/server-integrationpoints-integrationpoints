using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;
using Relativity.Services.FieldMapping;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class FieldCatalogController : ApiController
	{
		private readonly ICPHelper _helper;
		private readonly IServiceFactory _serviceFactory;

		public FieldCatalogController(ICPHelper helper, IServiceFactory serviceFactory)
		{
			_serviceFactory = serviceFactory;
			_helper = helper;
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve field catalog information.")]
		public HttpResponseMessage GetCurrentInstanceFields(int destinationWorkspaceId)
		{
			var fieldCatalogService = _serviceFactory.CreateFieldCatalogService(_helper);
			return GetFields(destinationWorkspaceId, fieldCatalogService);
		}

		private HttpResponseMessage GetFields(int destinationWorkspaceId, IFieldCatalogService fieldCatalogService)
		{
			ExternalMapping[] fieldsMap = fieldCatalogService.GetAllFieldCatalogMappings(destinationWorkspaceId);
			return Request.CreateResponse(HttpStatusCode.OK, fieldsMap, Configuration.Formatters.JsonFormatter);
		}
	}
}