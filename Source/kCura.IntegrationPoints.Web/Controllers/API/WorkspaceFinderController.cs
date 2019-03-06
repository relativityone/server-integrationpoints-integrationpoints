using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Utils;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.Models;
using Relativity.API;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFinderController : ApiController
	{
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly ICPHelper _helper;
		private readonly IHelperFactory _helperFactory;
		private readonly IHtmlSanitizerManager _htmlSanitizerManager;
		private readonly IManagerFactory _managerFactory;
		private readonly IWorkspaceContext _workspaceIdProvider;

		public WorkspaceFinderController(
			IManagerFactory managerFactory, 
			IContextContainerFactory contextContainerFactory, 
			IHtmlSanitizerManager htmlSanitizerManager, 
			ICPHelper helper,
			IHelperFactory helperFactory,
			IWorkspaceContext workspaceIdProvider)
		{
			_managerFactory = managerFactory;
			_contextContainerFactory = contextContainerFactory;
			_helper = helper;
			_helperFactory = helperFactory;
			_htmlSanitizerManager = htmlSanitizerManager;
			_workspaceIdProvider = workspaceIdProvider;
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve the workspace information.")]
		public HttpResponseMessage GetCurrentInstanceWorkspaces()
		{
			IWorkspaceManager workspaceManager =
				_managerFactory.CreateWorkspaceManager(_contextContainerFactory.CreateContextContainer(_helper, _helper.GetServicesManager()));
			return GetWorkspaces(workspaceManager);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve the workspace information.")]
		public HttpResponseMessage GetFederatedInstanceWorkspaces(int federatedInstanceId, [FromBody] object credentials)
		{
			var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceId, credentials.ToString());
			IWorkspaceManager workspaceManager =
				_managerFactory.CreateWorkspaceManager(_contextContainerFactory.CreateContextContainer(_helper, targetHelper.GetServicesManager()));
			return GetWorkspacesFromFederatedInstance(workspaceManager);
		}

		private HttpResponseMessage GetWorkspaces(IWorkspaceManager workspaceManager)
		{
			int currentWorkspaceId = _workspaceIdProvider.GetWorkspaceID();
			IEnumerable<WorkspaceDTO> userWorkspaces = workspaceManager.GetUserAvailableDestinationWorkspaces(currentWorkspaceId);
			return CreateResponseFromWorkspaceDTOs(userWorkspaces);
		}

		private HttpResponseMessage GetWorkspacesFromFederatedInstance(IWorkspaceManager workspaceManager)
		{
			IEnumerable<WorkspaceDTO> userWorkspaces =
				workspaceManager.GetUserActiveWorkspaces();
			return CreateResponseFromWorkspaceDTOs(userWorkspaces);
		}

		private HttpResponseMessage CreateResponseFromWorkspaceDTOs(IEnumerable<WorkspaceDTO> userWorkspaces)
		{
			WorkspaceModel[] workspaceModels = userWorkspaces.Select(x =>
				new WorkspaceModel
				{
					DisplayName = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(
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