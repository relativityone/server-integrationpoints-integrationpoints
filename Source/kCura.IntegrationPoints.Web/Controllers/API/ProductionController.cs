using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ProductionController : ApiController
	{
		private readonly IManagerFactory _managerFactory;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly ICPHelper _helper;

		public ProductionController(IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory,
			ICPHelper helper)
		{
			_contextContainerFactory = contextContainerFactory;
			_managerFactory = managerFactory;
			_helper = helper;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve export production list.")]
		public HttpResponseMessage GetProductionsForExport(int sourceWorkspaceArtifactId)
		{
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper);
			Core.Managers.IProductionManager productionManager = _managerFactory.CreateProductionManager(contextContainer);

			IEnumerable<ProductionDTO> productions = productionManager.GetProductionsForExport(sourceWorkspaceArtifactId);

			return Request.CreateResponse(HttpStatusCode.OK, productions.OrderBy(x => x.DisplayName));
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve import production list.")]
		public HttpResponseMessage GetProductionsForImport(int workspaceArtifactId, [FromBody] object credentials, int? federatedInstanceId = null)
		{
			string federatedInstanceCredentials = credentials?.ToString() ?? string.Empty;
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper);
			Core.Managers.IProductionManager productionManager = _managerFactory.CreateProductionManager(contextContainer);
			
			IEnumerable<ProductionDTO> productions = productionManager.GetProductionsForImport(workspaceArtifactId, federatedInstanceId, federatedInstanceCredentials);

			return Request.CreateResponse(HttpStatusCode.OK, productions.Where(y => !string.IsNullOrEmpty(y.DisplayName)).OrderBy(y => y.DisplayName));
		}
	}
}