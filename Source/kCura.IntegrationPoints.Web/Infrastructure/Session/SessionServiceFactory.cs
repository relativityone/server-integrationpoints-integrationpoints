using System;
using System.Web;
using System.Web.SessionState;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Infrastructure.Session
{
	internal class SessionServiceFactory
	{
		private const string _SESSION_KEY = "__WEB_SESSION_KEY__";

		public static ISessionService GetSessionService(Func<ICPHelper> helperFactory)
		{
			ISessionService sessionService = GetOrCreateSessionService(helperFactory);
			UpdateHttpSessionStateSessionService(sessionService);
			return sessionService;
		}

		private static ISessionService GetOrCreateSessionService(Func<ICPHelper> helperFactory)
		{
			HttpSessionState sessionState = HttpContext.Current.Session;
			var sessionService = sessionState?[_SESSION_KEY] as ISessionService;
			return sessionService ?? new SessionService(helperFactory());
		}

		private static void UpdateHttpSessionStateSessionService(ISessionService sessionService)
		{
			HttpSessionState sessionState = HttpContext.Current.Session;

			if (sessionState == null)
			{
				return;
			}
			sessionState[_SESSION_KEY] = sessionService;
		}
	}
}