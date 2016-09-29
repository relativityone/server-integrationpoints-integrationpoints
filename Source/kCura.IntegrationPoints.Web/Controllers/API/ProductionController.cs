using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ProductionController : ApiController
	{
		private readonly IProductionService _productionService;

		public ProductionController(IProductionService productionService)
		{
			_productionService = productionService;
		}

		[HttpGet]
		public HttpResponseMessage GetProductions(int sourceWorkspaceArtifactId)
		{
			try
			{
				var productions = _productionService.GetProductions(sourceWorkspaceArtifactId);

				return Request.CreateResponse(HttpStatusCode.OK, productions.OrderBy(x => x.DisplayName));
			}
			catch (Exception ex)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
			}
		}
	}
}