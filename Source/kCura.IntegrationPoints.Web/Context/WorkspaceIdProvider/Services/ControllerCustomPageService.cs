using System;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider.Services
{
	internal class ControllerCustomPageService : IWorkspaceService
	{
		private readonly ISessionService _sessionService;
		private readonly IAPILog _logger;

		public ControllerCustomPageService(ISessionService sessionService, IAPILog logger)
		{
			_sessionService = sessionService;
			_logger = logger.ForContext<ControllerCustomPageService>();
		}

		public int GetWorkspaceID()
		{
			try
			{
				return _sessionService.WorkspaceID;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ControllerCustomPageService failed when retrieving workspaceId");
				return 0;
			}
		}
	}
}