using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFinderController : ApiController
	{
		private readonly IRSAPIClient _context;

		public WorkspaceFinderController(WebClientFactory factory)
		{
			_context = factory.CreateEddsClient();
		}

		[HttpGet]
		public HttpResponseMessage Get()
		{
			try
			{
				var results = WorkspaceModel.GetWorkspaceModels(_context);
				return Request.CreateResponse(HttpStatusCode.OK, results);
			}
			catch (Exception)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new List<WorkspaceModel>());
			}
		}
	}
}