using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;

namespace kCura.IntegrationPoints.Core.Authentication
{
	public class OAuth2ClientFactory : IOAuth2ClientFactory
	{
		private readonly IHelper _helper;
		private readonly IAPILog _logger;

		public OAuth2ClientFactory(IHelper helper)
		{
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<OAuth2ClientFactory>();
		}

		public OAuth2Client GetOauth2Client(int userId)
		{
			using (var oAuth2ClientManager = _helper.GetServicesManager().CreateProxy<IOAuth2ClientManager>(ExecutionIdentity.System))
			{
				OAuth2Client client;
				string clientName = GenerateClientName(userId); 

				try
				{
					IEnumerable<OAuth2Client> clients = oAuth2ClientManager.ReadAllAsync().ConfigureAwait(false).GetAwaiter().GetResult();
					client = clients.SingleOrDefault(x => x.Name.Equals(clientName));

					if (client == null)
					{
						client = oAuth2ClientManager.CreateAsync(clientName, OAuth2Flow.ClientCredentials,
							new List<Uri>(), userId).ConfigureAwait(false).GetAwaiter().GetResult();
					}
				}
				catch (Exception ex)
				{
					LogGetOAuth2ClientError(ex);
					throw new InvalidOperationException($"Failed to retrieve OAuth2Client for user with id: {userId}", ex);
				}

				LogGetOAuth2ClientSuccess(userId);
				return client;
			}
		}

		private string GenerateClientName(int userId)
		{
			return $"{Constants.IntegrationPoints.OAUTH2_CLIENT_NAME_PREFIX} {userId}";
		}

		private void LogGetOAuth2ClientError(Exception ex)
		{
			_logger.LogError(ex, $"Error occured while getting OAuth2Client: {ex.Message}");
		}

		private void LogGetOAuth2ClientSuccess(int userId)
		{
			_logger.LogInformation($"OAuth2Client for user with id: {userId} created successfully");
		}
	}
}