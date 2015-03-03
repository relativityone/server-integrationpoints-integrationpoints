using System;
using System.Collections.Generic;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Syncronizer;
using kCura.IntegrationPoints.Core.Services.Tabs;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Models;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class CustodianController : ApiController
	{

		private readonly CustodianService _custodianService;
		
		public CustodianController(CustodianService custodianService)
		{

			_custodianService = custodianService;
		}


		[HttpPost]
		[Route("{workspaceID}/api/custodian/{id}")]
		public bool Post(int id)
		{
			return _custodianService.IsCustodian(id);
		}

	}
}
