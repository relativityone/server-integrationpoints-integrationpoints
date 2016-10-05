using System.Net;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication
{
	public interface ICredentialProvider
	{
		NetworkCredential Authenticate(CookieContainer cookieContainer);
	}
}
