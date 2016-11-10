using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFinderController : ApiController
	{
		private readonly IHtmlSanitizerManager _htmlSanitizerManager;
		private readonly IWorkspaceManager _workspaceManager;

		public WorkspaceFinderController(IWorkspaceManager workspaceManager, IHtmlSanitizerManager htmlSanitizerManager)
		{
			_workspaceManager = workspaceManager;
			_htmlSanitizerManager = htmlSanitizerManager;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve the workspace information.")]
		public HttpResponseMessage Get()
		{
			IEnumerable<WorkspaceDTO> userWorkspaces = _workspaceManager.GetUserWorkspaces();

			WorkspaceModel[] workspaceModels = userWorkspaces.Select(x =>
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