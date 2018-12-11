using System;
using kCura.IntegrationPoints.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Services
{
	public class ControllerCustomPageService : IWorkspaceService
	{
		private readonly ISessionService _sessionService;
		private readonly IAPILog _logger;

		public ControllerCustomPageService(ISessionService sessionService, IHelper helper)
		{
			_sessionService = sessionService;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ControllerCustomPageService>();
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