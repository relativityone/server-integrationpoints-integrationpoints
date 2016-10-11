using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Attributes;

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
		[LogApiExceptionFilter(Message = "Unable to retrieve accessible RDO list.")]
		public HttpResponseMessage Get()
		{
			var list = _rdoFilter.GetAllViewableRdos().Select(x => new { name = x.Name, value = x.DescriptorArtifactTypeID }).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, list);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to object type.")]
		public HttpResponseMessage Get(int id)
		{
			var list = _query.GetObjectType(id);
			return Request.CreateResponse(HttpStatusCode.OK, new {name = list.Name, value = list.DescriptorArtifactTypeID});
		}
	}
}