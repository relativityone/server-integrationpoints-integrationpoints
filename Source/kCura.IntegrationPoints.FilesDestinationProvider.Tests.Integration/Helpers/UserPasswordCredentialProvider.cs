using System.Net;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal class UserPasswordCredentialProvider : ICredentialProvider
	{
		private readonly ConfigSettings _configSettings;

		public UserPasswordCredentialProvider(ConfigSettings configSettings)
		{
			_configSettings = configSettings;
		}

		public NetworkCredential Authenticate(CookieContainer cookieContainer)
		{
			return LoginHelper.LoginUsernamePassword(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, cookieContainer);
		}
	}
}
