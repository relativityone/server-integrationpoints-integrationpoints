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
		private readonly IntegrationPointReader _integrationPointReader;
		public FieldMapController(IntegrationPointReader integrationPointReader)
		{
			_integrationPointReader = integrationPointReader;
		}


		[Route("{workspaceID}/api/FieldMap/{id}")]

	public HttpResponseMessage Get(string id)
		{
			
			int _id = 0; 
			Int32.TryParse(id,out _id);
			if (_id != 0)
			{
				var fieldsmap = _integrationPointReader.GetFieldMap(_id);
				return Request.CreateResponse(HttpStatusCode.OK, fieldsmap, Configuration.Formatters.JsonFormatter);
			}

			return Request.CreateResponse(HttpStatusCode.OK, string.Empty, Configuration.Formatters.JsonFormatter); 
		}

    }
}
