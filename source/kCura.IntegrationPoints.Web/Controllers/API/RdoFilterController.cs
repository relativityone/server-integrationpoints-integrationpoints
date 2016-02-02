using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class RdoFilterController : ApiController
	{
		private readonly Core.Models.RdoFilter _rdoFilter;
		private readonly RSAPIRdoQuery _query;
		public RdoFilterController(Core.Models.RdoFilter rdoFilter, RSAPIRdoQuery query)
		{
			_rdoFilter = rdoFilter;
			_query = query;
		}

		// GET api/<controller>
		[HttpGet]
		public HttpResponseMessage Get()
		{
			var list = _rdoFilter.FilterRdo().Select(x => new { name = x.Name, value = x.DescriptorArtifactTypeID }).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, list);
		}
	}
}