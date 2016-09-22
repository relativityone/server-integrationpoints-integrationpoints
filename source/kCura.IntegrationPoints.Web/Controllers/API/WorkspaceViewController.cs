using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Extensions;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceViewController : ApiController
	{
		#region Fields

		private readonly IViewService _viewService;
		private readonly IErrorRepository _errorRepository;

		#endregion //Fields

		#region Constructors

		public WorkspaceViewController(IViewService viewService, IRepositoryFactory repositoryFactory)
		{
			_viewService = viewService;
			_errorRepository = repositoryFactory.GetErrorRepository();
		}

		#endregion //Constructors

		#region Methods

		[HttpGet]
		public HttpResponseMessage GetViewsByWorkspaceAndArtifactType(int workspaceId, int artifactTypeId)
		{
			try
			{
				List<ViewDTO> views =_viewService.GetViewsByWorkspaceAndArtifactType(workspaceId, artifactTypeId);
				return Request.CreateResponse(HttpStatusCode.OK, views);
			}
			catch (Exception ex)
			{
				this.HandleError(workspaceId, _errorRepository, ex,
					$"Unable to Views for {workspaceId} workspace and artifact type {artifactTypeId}. Please contact the system administrator.");
				return Request.CreateResponse(HttpStatusCode.InternalServerError);
			}
		}

		#endregion Methods
	}
}