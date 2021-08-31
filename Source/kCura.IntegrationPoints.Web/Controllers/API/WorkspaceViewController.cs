using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceViewController : ApiController
	{
		private readonly IViewService _viewService;
		
		public WorkspaceViewController(IViewService viewService)
		{
			_viewService = viewService;
		}
		
		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve View list.")]
		public HttpResponseMessage GetViewsByWorkspaceAndArtifactType(int workspaceId, int artifactTypeId)
		{
			List<ViewDTO> views =_viewService.GetViewsByWorkspaceAndArtifactType(workspaceId, artifactTypeId);
			return Request.CreateResponse(HttpStatusCode.OK, views);
		}
	}
}