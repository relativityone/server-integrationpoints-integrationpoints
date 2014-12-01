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
		public HttpResponseMessage Get()
		{
			var list = _rdoFilter.FilterRdo().Select(x => new { Name = x.Name, Value = x.ArtifactID }).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, list);

		}

	}
}