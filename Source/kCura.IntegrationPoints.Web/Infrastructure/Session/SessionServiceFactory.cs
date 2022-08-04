using Relativity.API;
using System;
using System.Web;

namespace kCura.IntegrationPoints.Web.Infrastructure.Session
{
    internal static class SessionServiceFactory
    {
        private const string _SESSION_KEY = "__WEB_SESSION_KEY__";

        public static ISessionService GetSessionService(
            Func<ICPHelper> helperFactory,
            Func<IAPILog> loggerFactory,
            HttpContextBase httpContext)
        {
            HttpSessionStateBase sessionState = httpContext.Session;

            ISessionService sessionService = GetOrCreateSessionService(helperFactory, loggerFactory, sessionState);
            UpdateHttpSessionStateSessionService(sessionService, sessionState);
            return sessionService;
        }

        private static ISessionService GetOrCreateSessionService(
            Func<ICPHelper> helperFactory,
            Func<IAPILog> loggerFactory,
            HttpSessionStateBase sessionState)
        {
            var sessionService = sessionState?[_SESSION_KEY] as ISessionService;
            return sessionService ?? new SessionService(helperFactory(), loggerFactory());
        }

        private static void UpdateHttpSessionStateSessionService(
            ISessionService sessionService,
            HttpSessionStateBase sessionState)
        {
            if (sessionState == null)
            {
                return;
            }
            sessionState[_SESSION_KEY] = sessionService;
        }
    }
}