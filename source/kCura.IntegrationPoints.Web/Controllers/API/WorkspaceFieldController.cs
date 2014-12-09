using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using kCura.IntegrationPoints.Core.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFieldController : ApiController
    {


			// GET api/<controller>
			[Route("{workspaceID}/api/WorkspaceField/")]
			public HttpResponseMessage Get()
			{
				var list = new List<FieldEntry>()
				{
					new FieldEntry() {DisplayName = "Object", FieldIdentifier= "1"},
					new FieldEntry() {DisplayName= "Document", FieldIdentifier= "3"},
					new FieldEntry() {DisplayName= "Rdo", FieldIdentifier= "2"},
				};
					return Request.CreateResponse(HttpStatusCode.OK, list,Configuration.Formatters.JsonFormatter);
			}

    }

	
}
