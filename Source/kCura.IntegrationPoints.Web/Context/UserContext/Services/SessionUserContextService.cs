using System;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Context.UserContext.Services
{
	public class SessionUserContextService : IUserContextService
	{
		private readonly ISessionService _sessionService;
		private readonly IAPILog _logger;

		public SessionUserContextService(ISessionService sessionService, IAPILog logger)
		{
			_sessionService = sessionService;
			_logger = logger.ForContext<SessionUserContextService>();
		}

		public int GetUserID()
		{
			try
			{
				return _sessionService.UserID;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{nameof(ISessionService)} failed when retrieving {nameof(ISessionService.UserID)}");
				return 0;
			}
		}

		public int GetWorkspaceUserID()
		{
			try
			{
				return _sessionService.WorkspaceUserID;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{nameof(ISessionService)} failed when retrieving {nameof(ISessionService.WorkspaceUserID)}");
				return 0;
			}
		}
	}
}