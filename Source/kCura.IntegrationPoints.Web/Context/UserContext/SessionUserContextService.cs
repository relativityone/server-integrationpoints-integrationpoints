using System;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Context.UserContext
{
	public class SessionUserContextService : IUserContext
	{
		private readonly ISessionService _sessionService;
		private readonly IAPILog _logger;
		private readonly IUserContext _nextUserContextService;

		public SessionUserContextService(
			ISessionService sessionService, 
			IAPILog logger, 
			IUserContext nextUserContextService)
		{
			_sessionService = sessionService;
			_logger = logger.ForContext<SessionUserContextService>();
			_nextUserContextService = nextUserContextService;
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
			}

			return _nextUserContextService.GetUserID();
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
			}

			return _nextUserContextService.GetWorkspaceUserID();
		}
	}
}