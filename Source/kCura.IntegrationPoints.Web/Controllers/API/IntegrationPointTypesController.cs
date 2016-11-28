using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class IntegrationPointTypesController : ApiController
	{
		private readonly IIntegrationPointTypeService _integrationPointTypeService;

		public IntegrationPointTypesController(IIntegrationPointTypeService integrationPointTypeService)
		{
			_integrationPointTypeService = integrationPointTypeService;
		}


		[HttpGet]
		public HttpResponseMessage Get()
		{
			var types = _integrationPointTypeService.GetAllIntegrationPointTypes();
			var result = types.Select(x => new IntegrationPointTypeModel
			{
				Name = x.Name,
				ArtifactId = x.ArtifactId,
				Identifier = x.Identifier
			});
			return Request.CreateResponse(HttpStatusCode.OK, result);
		}
	}
}