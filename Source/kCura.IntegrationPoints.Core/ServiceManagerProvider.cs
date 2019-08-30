using System.Net;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Authentication.CredentialProvider;
using kCura.IntegrationPoints.Core.Factories;

namespace kCura.IntegrationPoints.Core
{
	public class ServiceManagerProvider : IServiceManagerProvider
	{
		private readonly IConfig _config;
		private readonly ICredentialProvider _credentialProvider;

		public ServiceManagerProvider(
			IConfigFactory configFactory,
			ICredentialProvider credentialProvider,
			ISqlServiceFactory sqlServiceFactory)
		{
			Apps.Common.Config.Manager.Settings.Factory = sqlServiceFactory;
			_config = configFactory.Create();
			_credentialProvider = credentialProvider;
		}

		public TManager Create<TManager, TFactory>() where TFactory : IServiceManagerFactory<TManager>, new()
		{
			WinEDDS.Config.ProgrammaticServiceURL = _config.WebApiPath;

			var cookieContainer = new CookieContainer();
			NetworkCredential credentials = _credentialProvider.Authenticate(cookieContainer);

			return (new TFactory()).Create(credentials, cookieContainer, _config.WebApiPath);
		}
	}
}
