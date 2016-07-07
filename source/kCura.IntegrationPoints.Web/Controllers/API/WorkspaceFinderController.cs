using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class WorkspaceFinderController : ApiController
    {
        private readonly IRSAPIClient _context;
	    private readonly IHtmlSanitizerManager _htmlSanitizerManager;
	    private readonly IErrorRepository _errorRepository;

	    public WorkspaceFinderController(WebClientFactory factory, IRepositoryFactory repositoryFactory, IHtmlSanitizerManager htmlSanitizerManager)
	    {
		    _errorRepository = repositoryFactory.GetErrorRepository(factory.WorkspaceId);
			_context = factory.CreateEddsClient();
		    _htmlSanitizerManager = htmlSanitizerManager;
        }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            try
            {
                var results = WorkspaceModel.GetWorkspaceModels(_context, _htmlSanitizerManager);
                return Request.CreateResponse(HttpStatusCode.OK, results);
            }
            catch (Exception exception)
            {
				ErrorDTO error = new ErrorDTO()
				{
					Message = "Unable to retrieve the workspace informations. Please contract the system administrator.",
					FullText = $"{exception.Message}{Environment.NewLine}{exception.StackTrace}",
				};
				_errorRepository.Create(new[] { error });
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new List<WorkspaceModel>());
            }
        }
    }
}