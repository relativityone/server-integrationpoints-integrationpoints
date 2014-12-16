using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFieldController : ApiController
    {

		private readonly RdoSynchronizer _rdosynchronizer;

		public WorkspaceFieldController(RdoSynchronizer rdosynchronizer)
			{
				_rdosynchronizer = rdosynchronizer;
				
			}
			// GET api/<controller
			[Route("{workspaceID}/api/WorkspaceField/")]
		public HttpResponseMessage Get(string json)
			{
				//int artifactid = 0;
				//Int32.TryParse(artifactTypeId, out artifactid);

				var fieldsForRdo = _rdosynchronizer.GetFields(json); 
				return Request.CreateResponse(HttpStatusCode.OK, fieldsForRdo, Configuration.Formatters.JsonFormatter);
			}


		

    }

	
}
