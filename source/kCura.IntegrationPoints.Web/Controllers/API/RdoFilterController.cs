using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class RdoFilterController : ApiController
	{
		private Core.Models.RdoFilter _rdoFilter;

		public RdoFilterController(Core.Models.RdoFilter rdoFilter)
		{
			_rdoFilter = rdoFilter;
		}
		
		// GET api/<controller>
		[Route("{workspaceID}/api/RdoFilter/")]
		public HttpResponseMessage Get()
		{
			var list = _rdoFilter.FilterRdo().Select(x => new { name = x.Name, value = x.ArtifactTypeID }).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, list);
		}

	}
}