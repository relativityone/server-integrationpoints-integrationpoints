using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.DestinationTypes;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class DestinationTypeController : ApiController
	{
		private readonly IDestinationTypeFactory _factory;
		private readonly ICaseServiceContext _serviceContext;
		private readonly IObjectTypeQuery _rdoQuery;

		public DestinationTypeController(IDestinationTypeFactory factory,
			ICaseServiceContext serviceContext,
			RSAPIRdoQuery objectTypeQuery)
		{
			_factory = factory;
			_rdoQuery = objectTypeQuery;
			_serviceContext = serviceContext;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve destination provider type info.")]
		public HttpResponseMessage Get()
		{
			Dictionary<Guid, int> rdoTypesCache = _rdoQuery.GetRdoGuidToArtifactIdMap(_serviceContext.WorkspaceUserID);
			List<DestinationType> list = _factory.GetDestinationTypes().ToList();
			return Request.CreateResponse(HttpStatusCode.OK, list);
		}
	}
}
