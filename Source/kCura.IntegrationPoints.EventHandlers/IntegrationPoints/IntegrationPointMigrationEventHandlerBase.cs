using System.Collections.Generic;
using System.Linq;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public abstract class IntegrationPointMigrationEventHandlerBase : PostWorkspaceCreateEventHandlerBase
	{
		private ICaseServiceContext _workspaceTemplateServiceContext;

		protected ICaseServiceContext WorkspaceTemplateServiceContext
		{
			get
			{
				if (_workspaceTemplateServiceContext == null)
				{
					_workspaceTemplateServiceContext = ServiceContextFactory.CreateCaseServiceContext(Helper, TemplateWorkspaceID);
				}
				return _workspaceTemplateServiceContext;
			}
		}
	}
}