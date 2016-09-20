using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ProductionPrecedenceController : ApiController
	{
		private readonly IProductionPrecedenceService _productionPrecedenceService;

		public ProductionPrecedenceController(IProductionPrecedenceService productionPrecedenceService)
		{
			_productionPrecedenceService = productionPrecedenceService;
		}

		[HttpGet]
		public HttpResponseMessage GetProductionPrecedence(int sourceWorkspaceArtifactId)
		{
			try
			{
				var productions = _productionPrecedenceService.GetProductionPrecedence(sourceWorkspaceArtifactId);

				return Request.CreateResponse(HttpStatusCode.OK, productions.OrderBy(x => x.DisplayName));
			}
			catch (Exception ex)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
			}
		}
	}
}