using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;
using Relativity.Services.FieldMapping;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class FieldCatalogController : ApiController
	{
		private readonly IHelperFactory _helperFactory;
		private readonly ICPHelper _helper;
		private readonly IServiceFactory _serviceFactory;

		public FieldCatalogController(ICPHelper helper, IHelperFactory helperFactory, IServiceFactory serviceFactory)
		{
			_serviceFactory = serviceFactory;
			_helper = helper;
			_helperFactory = helperFactory;
		}

		[LogApiExceptionFilter(Message = "Unable to retrieve field catalog information.")]
		public HttpResponseMessage Get(int destinationWorkspaceId, int? federatedInstanceId = null)
		{
			var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceId);
			var fieldCatalogService = _serviceFactory.CreateFieldCatalogService(targetHelper);
			ExternalMapping[] fieldsMap = fieldCatalogService.GetAllFieldCatalogMappings(destinationWorkspaceId);
			return Request.CreateResponse(HttpStatusCode.OK, fieldsMap, Configuration.Formatters.JsonFormatter);
		}
	}
}
