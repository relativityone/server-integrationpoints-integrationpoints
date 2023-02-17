using kCura.IntegrationPoints.Web.Context.UserContext.Exceptions;
using Relativity.API;
using System.Runtime.CompilerServices;

namespace kCura.IntegrationPoints.Web.Context.UserContext
{
    public class NotFoundUserContextService : IUserContext
    {
        private readonly IAPILog _logger;

        public NotFoundUserContextService(IAPILog logger)
        {
            _logger = logger.ForContext<NotFoundUserContextService>();
        }

        public int GetUserID() => LogWarningAndThrowException();

        public int GetWorkspaceUserID() => LogWarningAndThrowException();
        private int LogWarningAndThrowException([CallerMemberName] string propertyName = "")
        {
            _logger.LogWarning("{propertyName} not found in user context", propertyName);
            throw new UserContextNotFoundException(propertyName);
        }
    }
}
