using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class WorkspaceFinderController : ApiController
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IHtmlSanitizerManager _htmlSanitizerManager;

        public WorkspaceFinderController(IRepositoryFactory repositoryFactory, IHtmlSanitizerManager htmlSanitizerManager)
        {
            _repositoryFactory = repositoryFactory;
            _htmlSanitizerManager = htmlSanitizerManager;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve the workspace information.")]
        public HttpResponseMessage Get()
        {
            IWorkspacesRepository repository = _repositoryFactory.GetWorkspacesRepository();

            WorkspaceModel[] workspaceModels = repository.RetrieveAllActive().Select(x =>
                new WorkspaceModel
                {
                    DisplayName = Utils.GetFormatForWorkspaceOrJobDisplay(
                        _htmlSanitizerManager.Sanitize(x.Name).CleanHTML,
                        x.ArtifactId
                    ),
                    Value = x.ArtifactId
                }
            ).ToArray();

            return Request.CreateResponse(HttpStatusCode.OK, workspaceModels);
        }
    }
}