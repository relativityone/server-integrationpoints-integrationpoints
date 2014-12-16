using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class FieldMapController : ApiController
    {
        //
        // GET: /FieldMap/
		private readonly IntegrationPointService _integrationPointReader;
		public FieldMapController(IntegrationPointService integrationPointReader)
		{
			_integrationPointReader = integrationPointReader;
		}


		[Route("{workspaceID}/api/FieldMap/{string}")]

		public HttpResponseMessage Get(string artifactTypeId)
		{
			
			int _id = 0;
			Int32.TryParse(artifactTypeId, out _id);
			if (_id != 0)
			{
				var fieldsmap = _integrationPointReader.GetFieldMap(_id);
				return Request.CreateResponse(HttpStatusCode.OK, fieldsmap, Configuration.Formatters.JsonFormatter);
			}

			return Request.CreateResponse(HttpStatusCode.OK, string.Empty, Configuration.Formatters.JsonFormatter); 
		}

    }
}
