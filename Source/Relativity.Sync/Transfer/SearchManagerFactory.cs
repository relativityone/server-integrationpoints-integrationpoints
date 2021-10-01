using System;
using System.Net;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using kCura.WinEDDS.Api;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;
using Relativity.DataExchange;
using Relativity.Sync.Configuration;
using Relativity.Sync.Authentication;

namespace Relativity.Sync.Transfer
{
	internal class SearchManagerFactory : ISearchManagerFactory
	{
		private readonly IInstanceSettings _instanceSettings;
		private readonly SyncJobParameters _syncJobParameters;
		private readonly IAuthTokenGenerator _tokenGenerator;
		private readonly IUserContextConfiguration _userContextConfiguration;
		private readonly Lazy<Task<ISearchManager>> _searchManagerFactoryLazy;

		// If you have a long running process and you have to create many dynamic proxies, you should make sure to reuse the same ProxyGenerator instance.
		// If not, be aware that you will then bypass the caching mechanism. Side effects are high CPU usage and constant increase in memory consumption.
		// https://github.com/castleproject/Core/blob/master/docs/dynamicproxy.md
		private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

		public SearchManagerFactory(IInstanceSettings instanceSettings, SyncJobParameters syncJobParameters, IAuthTokenGenerator tokenGenerator, IUserContextConfiguration userContextConfiguration)
		{
			_instanceSettings = instanceSettings;
			_syncJobParameters = syncJobParameters;
			_tokenGenerator = tokenGenerator;
			_userContextConfiguration = userContextConfiguration;

			_searchManagerFactoryLazy = new Lazy<Task<ISearchManager>>(WrappedSearchManagerFactoryAsync);
		}

		public Task<ISearchManager> CreateSearchManagerAsync()
		{
			return _searchManagerFactoryLazy.Value;
		}

		private async Task<ISearchManager> WrappedSearchManagerFactoryAsync()
		{
			ISearchManager searchManager = await SearchManagerFactoryAsync().ConfigureAwait(false);
			SearchManagerInterceptor searchManagerInterceptor = new SearchManagerInterceptor(SearchManagerFactoryAsync);

			return _proxyGenerator.CreateInterfaceProxyWithTargetInterface<ISearchManager>(searchManager, searchManagerInterceptor);
		}

		private async Task<ISearchManager> SearchManagerFactoryAsync()
		{
			kCura.WinEDDS.Config.ProgrammaticServiceURL = await _instanceSettings.GetWebApiPathAsync().ConfigureAwait(false);

			string authToken = await _tokenGenerator.GetAuthTokenAsync(_userContextConfiguration.ExecutingUserId).ConfigureAwait(false);

			CookieContainer cookieContainer = new CookieContainer();
			IRunningContext runningContext = new RunningContext()
			{
				ApplicationName = _syncJobParameters.SyncApplicationName
			};
			NetworkCredential credentials = LoginHelper.LoginUsernamePassword(AuthConstants._RELATIVITY_BEARER_USERNAME, authToken, cookieContainer, runningContext);

			return new SearchManager(credentials, cookieContainer);
		}
	}
}
