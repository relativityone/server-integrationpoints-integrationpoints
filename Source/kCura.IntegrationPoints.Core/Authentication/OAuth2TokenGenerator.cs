using System;
using System.Collections.Generic;
using System.Threading;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.WinEDDS.Credentials;
using Relativity.API;
using Relativity.OAuth2Client.Interfaces;
using Relativity.Services.Security.Models;
using ITokenProvider = Relativity.OAuth2Client.Interfaces.ITokenProvider;

namespace kCura.IntegrationPoints.Core.Authentication
{
	public class OAuth2TokenGenerator : IAuthTokenGenerator
	{
		private readonly IAPILog _logger;
		private readonly IOAuth2ClientFactory _oAuth2ClientFactory;
		private readonly ITokenProviderFactoryFactory _tokenProviderFactory;
		private readonly CurrentUser _contextUser;
		private readonly CancellationTokenSource _cancellationTokenSource;

		public OAuth2TokenGenerator(IHelper helper, IOAuth2ClientFactory oAuth2ClientFactory, ITokenProviderFactoryFactory tokenProviderFactory, CurrentUser contextUser)
		{
			_oAuth2ClientFactory = oAuth2ClientFactory;
			_contextUser = contextUser;
			_tokenProviderFactory = tokenProviderFactory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<OAuth2TokenGenerator>();
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public string GetAuthToken()
		{
			try
			{
				OAuth2Client oauth2Client = _oAuth2ClientFactory.GetOauth2Client(_contextUser.ID);
				ITokenProvider tokenProvider = CreateTokenProvider(oauth2Client);
				string token = tokenProvider.GetAccessTokenAsync(_cancellationTokenSource.Token).ConfigureAwait(false)
					.GetAwaiter().GetResult();

				return token;
			}
			catch (Exception exception)
			{
				LogGetAuthTokenError(exception);
				throw;
			}
		}

		private ITokenProvider CreateTokenProvider(OAuth2Client client)
		{
			ITokenProviderFactory providerFactory = _tokenProviderFactory.Create(GetRelativityStsUri(), client.Id, client.Secret);
			ITokenProvider tokenProvider = providerFactory.GetTokenProvider("WebApi", new List<string>() { "UserInfoAccess" });
			RelativityWebApiCredentialsProvider.Instance().SetProvider(new OAuth2ClientCredentials(tokenProvider));
			return tokenProvider;
		}

		private Uri GetRelativityStsUri()
		{
			string relativityInstance = ExtensionPointServiceFinder.ServiceUriProvider
				.AuthenticationUri().ToString();
			var relativityStsUri = new Uri(System.IO.Path.Combine(relativityInstance, Constants.IntegrationPoints.RELATIVITY_AUTH_ENDPOINT));
			LogRelativityStsUrl(relativityInstance);
			return relativityStsUri;
		}

		private void LogRelativityStsUrl(string relativityInstance)
		{
			_logger.LogDebug($"Relativity Service Url: {relativityInstance}");
		}

		private void LogGetAuthTokenError(Exception exception)
		{
			_logger.LogError(exception,
				$"Failed to get Authentication Token for user with ID: {_contextUser}. Details: {exception.Message}");
		}
	}
}