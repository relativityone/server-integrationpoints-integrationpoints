using System.Net;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public interface IManagerFactory<out TManager>
	{
		TManager Create(ICredentials credentials, CookieContainer cookieContainer);
	}
}
