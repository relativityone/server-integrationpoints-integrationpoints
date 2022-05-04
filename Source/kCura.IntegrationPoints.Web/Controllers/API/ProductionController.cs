using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity;
using PageLevelNumbering = Relativity.Productions.Services.PageLevelNumbering;
using Production = Relativity.Productions.Services.Production;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ProductionController : ApiController
	{
		private readonly IManagerFactory _managerFactory;
		private readonly IProductionManager _productionManager;

		public ProductionController(
			IManagerFactory managerFactory,
			IProductionManager productionManager)
		{
			_managerFactory = managerFactory;
			_productionManager = productionManager;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve export production list.")]
		public HttpResponseMessage GetProductionsForExport(int sourceWorkspaceArtifactId)
        {
            IEnumerable<ProductionDTO> productions = _productionManager.GetProductionsForExport(sourceWorkspaceArtifactId);
            return Request.CreateResponse(HttpStatusCode.OK, productions.OrderBy(x => x.DisplayName));
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve import production list.")]
		public HttpResponseMessage GetProductionsForImport(int workspaceArtifactId, [FromBody] object credentials, int? federatedInstanceId = null)
		{
			string federatedInstanceCredentials = credentials?.ToString() ?? string.Empty;

			IEnumerable<ProductionDTO> productions = _productionManager.GetProductionsForImport(workspaceArtifactId, federatedInstanceId, federatedInstanceCredentials);

			return Request.CreateResponse(HttpStatusCode.OK, productions.Where(y => !string.IsNullOrEmpty(y.DisplayName)).OrderBy(y => y.DisplayName));
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to create new production.")]
		public HttpResponseMessage CreateProductionSet(string productionName, int workspaceArtifactId, [FromBody] object credentials, int? federatedInstanceId = null)
		{
			var numbering = new PageLevelNumbering()
			{
				BatesPrefix = "REL"
			};

			var production = new Production()
			{
				Name = productionName,
				Numbering = numbering
			};

			int productionArtifactId = _productionManager.CreateSingle(workspaceArtifactId, production);

			return Request.CreateResponse(HttpStatusCode.OK, productionArtifactId);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve production permissions.")]
		public HttpResponseMessage CheckProductionAddPermission([FromBody] object credentials, int workspaceArtifactId, int? federatedInstanceId = null)
		{
			IPermissionManager permissionManager = _managerFactory.CreatePermissionManager();
			bool hasPermission = permissionManager.UserHasArtifactTypePermission(workspaceArtifactId, (int)ArtifactType.Production, ArtifactPermission.Create);
			return Request.CreateResponse(HttpStatusCode.OK, hasPermission);
		}
	}
}