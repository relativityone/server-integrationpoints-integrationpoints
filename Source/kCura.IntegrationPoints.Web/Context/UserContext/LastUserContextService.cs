using System.Runtime.CompilerServices;
using kCura.IntegrationPoints.Web.Context.UserContext.Exceptions;

namespace kCura.IntegrationPoints.Web.Context.UserContext
{
	public class LastUserContextService : IUserContext
	{
		public int GetUserID() => ThrowException();

		public int GetWorkspaceUserID() => ThrowException();

		private static int ThrowException([CallerMemberName] string propertyName = "")
		{
			throw new UserContextNotFoundException(propertyName);
		}
	}
}