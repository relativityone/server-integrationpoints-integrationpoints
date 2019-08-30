using System.Net;

namespace kCura.IntegrationPoints.Core.Authentication.CredentialProvider
{
	public interface ICredentialProvider
	{
		NetworkCredential Authenticate(CookieContainer cookieContainer);
	}
}
