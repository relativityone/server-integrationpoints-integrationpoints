using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.SourceTypes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SourceTypeController : ApiController
	{
		private readonly SourceTypeFactory _factory;
		public SourceTypeController(SourceTypeFactory factory)
		{
			_factory = factory;
		}

		[HttpGet]
		public HttpResponseMessage Get()
		{
			var list = _factory.GetSourceTypes().Select(x => new {name = x.Name, value = x.ID});
			return Request.CreateResponse(HttpStatusCode.OK, list);
		}
	}
}
