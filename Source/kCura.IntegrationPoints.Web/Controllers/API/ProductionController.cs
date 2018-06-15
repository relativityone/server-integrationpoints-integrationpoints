using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client;
using Relativity.API;
using PageLevelNumbering = Relativity.Productions.Services.PageLevelNumbering;
using Production = Relativity.Productions.Services.Production;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ProductionController : ApiController
	{
		private readonly IManagerFactory _managerFactory;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly ICPHelper _helper;
		private readonly IHelperFactory _helperFactory;

		public ProductionController(IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory,
			ICPHelper helper, IHelperFactory helperFactory)
		{
			_contextContainerFactory = contextContainerFactory;
			_managerFactory = managerFactory;
			_helper = helper;
		    _helperFactory = helperFactory;
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

	    [HttpPost]
	    [LogApiExceptionFilter(Message = "Unable to create new production.")]
	    public HttpResponseMessage CreateProductionSet(string productionName, int workspaceArtifactId, [FromBody] object credentials, int? federatedInstanceId = null)
	    {
            IHelper targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceId, credentials?.ToString());
            IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper, targetHelper.GetServicesManager());
            Core.Managers.IProductionManager productionManager = _managerFactory.CreateProductionManager(contextContainer);

	        var numbering = new PageLevelNumbering()
	        {
	            BatesPrefix = "REL"
	        };

	        var production = new Production()
	        {
	            Name = productionName,
                Numbering = numbering
	        };
            
            int productionArtifactId = productionManager.CreateSingle(workspaceArtifactId, production);

	        return Request.CreateResponse(HttpStatusCode.OK, productionArtifactId);
	    }

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve production permissions.")]
		public HttpResponseMessage CheckProductionAddPermission([FromBody] object credentials, int workspaceArtifactId, int? federatedInstanceId = null)
		{
			IHelper targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceId, credentials?.ToString());
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(targetHelper, targetHelper.GetServicesManager());
			IPermissionManager permissionManager = _managerFactory.CreatePermissionManager(contextContainer);
			bool hasPermission = permissionManager.UserHasArtifactTypePermission(workspaceArtifactId, (int)ArtifactType.Production, ArtifactPermission.Create);
			return Request.CreateResponse(HttpStatusCode.OK, hasPermission);
		}
    }
}