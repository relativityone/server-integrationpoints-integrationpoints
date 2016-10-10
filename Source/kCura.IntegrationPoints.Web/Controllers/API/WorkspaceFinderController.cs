using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class WorkspaceFinderController : ApiController
    {
        private readonly IRSAPIClient _context;
	    private readonly IHtmlSanitizerManager _htmlSanitizerManager;

	    public WorkspaceFinderController(WebClientFactory factory, IHtmlSanitizerManager htmlSanitizerManager)
	    {
			_context = factory.CreateEddsClient();
		    _htmlSanitizerManager = htmlSanitizerManager;
	    }

        [HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve the workspace information.")]
		public HttpResponseMessage Get()
        {
            var results = WorkspaceModel.GetWorkspaceModels(_context, _htmlSanitizerManager);
            return Request.CreateResponse(HttpStatusCode.OK, results);
        }
    }
}