using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class RdoMetaController : ApiController
	{
		private readonly Core.Services.ObjectTypeService _service;
		public RdoMetaController(Core.Services.ObjectTypeService service)
		{
			_service = service;
		}
		[HttpGet]
		public HttpResponseMessage Get(int id)
		{
			var hasParent = _service.HasParent(id);
			return Request.CreateResponse(HttpStatusCode.OK, new {hasParent = hasParent});
		}
	}
}
