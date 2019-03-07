using System;
using System.Collections.Generic;
using System.IO;
using Relativity.API;
using Relativity.OAuth2Client.Interfaces;

namespace Relativity.Sync.Authentication
{
	internal sealed class OAuth2TokenGenerator : IAuthTokenGenerator
	{
		private const string _RELATIVITY_AUTH_ENDPOINT = "Identity/connect/token";
		private readonly IOAuth2ClientFactory _oAuth2ClientFactory;
		private readonly ITokenProviderFactoryFactory _tokenProviderFactory;
		private readonly IProvideServiceUris _provideServiceUris;
		private readonly IAPILog _logger;

		public OAuth2TokenGenerator(IOAuth2ClientFactory oAuth2ClientFactory, ITokenProviderFactoryFactory tokenProviderFactory, IProvideServiceUris provideServiceUris, IAPILog logger)
		{
			_oAuth2ClientFactory = oAuth2ClientFactory;
			_tokenProviderFactory = tokenProviderFactory;
			_provideServiceUris = provideServiceUris;
			_logger = logger;
		}

		public string GetAuthToken(int userId)
		{
			try
			{
				Relativity.Services.Security.Models.OAuth2Client oauth2Client = _oAuth2ClientFactory.GetOauth2Client(userId);
				ITokenProvider tokenProvider = CreateTokenProvider(oauth2Client);
				string token = tokenProvider.GetAccessTokenAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				return token;
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, $"Failed to get Authentication Token for user with ID: {userId}. Details: {exception.Message}");
				throw;
			}
		}

		private ITokenProvider CreateTokenProvider(Relativity.Services.Security.Models.OAuth2Client client)
		{
			ITokenProviderFactory providerFactory = _tokenProviderFactory.Create(GetRelativityStsUri(), client.Id, client.Secret);
			ITokenProvider tokenProvider = providerFactory.GetTokenProvider("WebApi", new List<string> { "UserInfoAccess" });
			return tokenProvider;
		}

		private Uri GetRelativityStsUri()
		{
			string relativityInstance = _provideServiceUris.AuthenticationUri().ToString();
			var relativityStsUri = new Uri(Path.Combine(relativityInstance, _RELATIVITY_AUTH_ENDPOINT));
			return relativityStsUri;
		}
	}
}