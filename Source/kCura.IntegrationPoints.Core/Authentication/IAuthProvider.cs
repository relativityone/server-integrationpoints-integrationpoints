using System.Net;

namespace kCura.IntegrationPoints.Core.Authentication
{
	public interface IAuthProvider
	{
		NetworkCredential LoginUsingAuthToken(string token, CookieContainer cookieContainer);
	}
}