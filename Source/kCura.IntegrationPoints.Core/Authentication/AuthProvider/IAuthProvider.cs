using System.Net;

namespace kCura.IntegrationPoints.Core.Authentication.AuthProvider
{
	public interface IAuthProvider
	{
		NetworkCredential LoginUsingAuthToken(string token, CookieContainer cookieContainer);
	}
}