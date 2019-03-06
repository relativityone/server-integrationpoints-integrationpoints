using kCura.IntegrationPoints.Web.Context.UserContext.Exceptions;
using Relativity.API;
using System.Runtime.CompilerServices;

namespace kCura.IntegrationPoints.Web.Context.UserContext
{
	public class LastUserContextService : IUserContext
	{
		private readonly IAPILog _logger;

		public LastUserContextService(IAPILog logger)
		{
			_logger = logger.ForContext<LastUserContextService>();
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