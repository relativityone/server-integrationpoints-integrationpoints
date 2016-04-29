﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class FieldMapController : ApiController
	{
		private readonly IIntegrationPointService _integrationPointReader;
		public FieldMapController(IIntegrationPointService integrationPointReader)
		{
			_integrationPointReader = integrationPointReader;
		}

		public HttpResponseMessage Get(int id)
		{
			var fieldsmap = _integrationPointReader.GetFieldMap(id);
			return Request.CreateResponse(HttpStatusCode.OK, fieldsmap, Configuration.Formatters.JsonFormatter);

		}

	}
}
