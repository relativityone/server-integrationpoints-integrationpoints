using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceViewController : ApiController
	{
		#region Fields

		private readonly IViewService _viewService;

		#endregion //Fields

		#region Constructors

		public WorkspaceViewController(IViewService viewService, IRepositoryFactory repositoryFactory)
		{
			_viewService = viewService;
		}

		#endregion //Constructors

		#region Methods

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve View list.")]
		public HttpResponseMessage GetViewsByWorkspaceAndArtifactType(int workspaceId, int artifactTypeId)
		{
			List<ViewDTO> views =_viewService.GetViewsByWorkspaceAndArtifactType(workspaceId, artifactTypeId);
			return Request.CreateResponse(HttpStatusCode.OK, views);
		}

		#endregion Methods
	}
}