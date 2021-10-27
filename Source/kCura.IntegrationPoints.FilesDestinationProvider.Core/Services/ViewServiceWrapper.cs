using System.Collections.Generic;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Toggles;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ViewServiceWrapper : IViewService
	{
		private readonly IViewService _viewService;

		public ViewServiceWrapper(IToggleProvider toggleProvider, IHelper helper, IServiceManagerProvider serviceManagerProvider)
		{
			_viewService = toggleProvider.IsEnabled<EnableKeplerizedImportAPIToggle>()
				? (IViewService)new ViewService(helper)
				: new WebAPIViewService(serviceManagerProvider, helper);
		}

		public List<ViewDTO> GetViewsByWorkspaceAndArtifactType(int workspceId, int artifactTypeId)
		{
			return _viewService.GetViewsByWorkspaceAndArtifactType(workspceId, artifactTypeId);
		}
	}
}
