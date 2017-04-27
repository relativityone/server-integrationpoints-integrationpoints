using System.Net;

namespace kCura.IntegrationPoints.Core.Authentication
{
	public interface ICredentialProvider
	{
		NetworkCredential Authenticate(CookieContainer cookieContainer);
	}
}
