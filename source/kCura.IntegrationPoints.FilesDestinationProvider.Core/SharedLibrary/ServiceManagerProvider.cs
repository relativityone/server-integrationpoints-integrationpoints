using System.Net;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public static class ServiceManagerProvider
	{
		public static TManager Create<TManager, TManagerFactory>(IConfig config, ICredentialProvider credentialProvider)
			where TManagerFactory : IManagerFactory<TManager>, new()
		{
			WinEDDS.Config.ProgrammaticServiceURL = config.WebApiPath;

			var cookieContainer = new CookieContainer();
			var credentials = credentialProvider.Authenticate(cookieContainer);

			var searchManager = (new TManagerFactory()).Create(credentials, cookieContainer);

			return searchManager;
		}
	}
}
