using System.Net;

namespace kCura.IntegrationPoints.Core.Authentication
{
	public interface IAuthProvider
	{
		NetworkCredential LoginUsingWinAuth(CookieContainer cookieContainer);
		NetworkCredential LoginUsingUserNameAndPassword(string username, string password, CookieContainer cookieContainer);
		NetworkCredential LoginUsingAuthToken(string token, CookieContainer cookieContainer);
	}
}