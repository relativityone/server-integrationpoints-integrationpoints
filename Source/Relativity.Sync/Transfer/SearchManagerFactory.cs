using System;
using System.Net;
using System.Threading.Tasks;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;
using Relativity.Sync.Configuration;
using Relativity.Sync.Authentication;
using kCura.WinEDDS.Api;

namespace Relativity.Sync.Transfer
{
	internal class SearchManagerFactory: ISearchManagerFactory
	{
		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";

		private readonly IInstanceSettings _instanceSettings;
		private readonly IAuthTokenGenerator _tokenGenerator;
		private readonly IUserContextConfiguration _userContextConfiguration;
		private readonly Lazy<Task<ISearchManager>> _searchManagerFactoryLazy;

		public SearchManagerFactory(IInstanceSettings instanceSettings, IAuthTokenGenerator tokenGenerator, IUserContextConfiguration userContextConfiguration)
		{
			_instanceSettings = instanceSettings;
			_tokenGenerator = tokenGenerator;
			_userContextConfiguration = userContextConfiguration;

			_searchManagerFactoryLazy = new Lazy<Task<ISearchManager>>(SearchManagerFactoryAsync);
		}

		public Task<ISearchManager> CreateSearchManagerAsync()
		{
			return _searchManagerFactoryLazy.Value;
		}

		public async Task<ISearchManager> SearchManagerFactoryAsync()
		{
			kCura.WinEDDS.Config.ProgrammaticServiceURL = await _instanceSettings.GetWebApiPathAsync().ConfigureAwait(false);

			string authToken = await _tokenGenerator.GetAuthTokenAsync(_userContextConfiguration.ExecutingUserId).ConfigureAwait(false);

			CookieContainer cookieContainer = new CookieContainer();
			NetworkCredential credentials = LoginHelper.LoginUsernamePassword(_RELATIVITY_BEARER_USERNAME, authToken, cookieContainer);

			return new SearchManager(credentials, cookieContainer);
		}
	}
}
