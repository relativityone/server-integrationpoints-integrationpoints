using System;
using System.Net;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Authentication.WebApi;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Data.Toggles;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core
{
	public class ServiceManagerProvider : IServiceManagerProvider
	{
		private readonly IConfig _config;
		private readonly IWebApiLoginService _credentialProvider;
		private readonly IToggleProvider _toggleProvider;

		public ServiceManagerProvider(
			IConfigFactory configFactory,
			IWebApiLoginService credentialProvider,
			ISqlServiceFactory sqlServiceFactory,
			IToggleProvider toggleProvider)
		{
			Apps.Common.Config.Manager.Settings.Factory = sqlServiceFactory;
			_config = configFactory.Create();
			_credentialProvider = credentialProvider;
			_toggleProvider = toggleProvider;
		}

		public TManager Create<TManager, TFactory>() where TFactory : IServiceManagerFactory<TManager>, new()
		{
			if (_toggleProvider.IsEnabled<EnableKeplerizedImportAPIToggle>())
			{
				throw new InvalidOperationException(
					"Keplerized Import API is on, ServiceManagerProvider.Create should not be called");
			}

			WinEDDS.Config.ProgrammaticServiceURL = _config.WebApiPath;

			var cookieContainer = new CookieContainer();
			NetworkCredential credentials = _credentialProvider.Authenticate(cookieContainer);

			return (new TFactory()).Create(credentials, cookieContainer, _config.WebApiPath);
		}
	}
}
