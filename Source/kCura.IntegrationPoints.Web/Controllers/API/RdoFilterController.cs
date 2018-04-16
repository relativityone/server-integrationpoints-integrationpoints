using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class RdoFilterController : ApiController
	{
		private readonly Core.Models.IRdoFilter _rdoFilter;
		private readonly IObjectTypeRepository _objectTypeRepository;

		public RdoFilterController(Core.Models.IRdoFilter rdoFilter, IObjectTypeRepository objectTypeRepository)
		{
			_rdoFilter = rdoFilter;
			_objectTypeRepository = objectTypeRepository;
		}

		// GET api/<controller>
		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve accessible RDO list.")]
		public HttpResponseMessage GetAllViewableRdos()
		{
			var list = _rdoFilter.GetAllViewableRdos().Select(x => new { name = x.Name, value = x.DescriptorArtifactTypeId }).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, list);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to object type.")]
		public HttpResponseMessage Get(int id)
		{
			var list = _objectTypeRepository.GetObjectType(id);
			
			return Request.CreateResponse(HttpStatusCode.OK, new {name = list.Name, value = list.DescriptorArtifactTypeId });
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to get default rdo type id")]
		public HttpResponseMessage GetDefaultRdoTypeId()
		{
			return Request.CreateResponse(HttpStatusCode.OK, (int)ArtifactType.Document);
		}
	}
}