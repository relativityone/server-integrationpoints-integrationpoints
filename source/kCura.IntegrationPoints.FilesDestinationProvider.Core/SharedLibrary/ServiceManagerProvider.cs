using System.Net;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{

	public class ServiceManagerProvider : IServiceManagerProvider
	{
		#region Fields

		private readonly IConfig _config;
		private readonly ICredentialProvider _credentialProvider;

		#endregion //Fields

		#region Constructors

		public ServiceManagerProvider(IConfigFactory configFactory, ICredentialProvider credentialProvider)
		{
			_config = configFactory.Create();
			_credentialProvider = credentialProvider;
		}

		#endregion //Constructors

		#region Methods

		public TManager Create<TManager, TFactory>() where TFactory : IManagerFactory<TManager>, new()
		{
			WinEDDS.Config.ProgrammaticServiceURL = _config.WebApiPath;

			var cookieContainer = new CookieContainer();
			var credentials = _credentialProvider.Authenticate(cookieContainer);

			return (new TFactory()).Create(credentials, cookieContainer);
		}

		#endregion //Methods
	}
}
