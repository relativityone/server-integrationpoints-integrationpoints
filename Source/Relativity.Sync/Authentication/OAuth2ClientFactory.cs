using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;

namespace Relativity.Sync.Authentication
{
	internal sealed class OAuth2ClientFactory : IOAuth2ClientFactory
	{
		private const string _OAUTH2_CLIENT_NAME_PREFIX = "F6B8C2B4B3E8465CA00775F699375D3C";
		private readonly IServicesMgr _servicesMgr;
		private readonly IAPILog _logger;

		public OAuth2ClientFactory(IServicesMgr servicesMgr, IAPILog logger)
		{
			_servicesMgr = servicesMgr;
			_logger = logger;
		}

		public Services.Security.Models.OAuth2Client GetOauth2Client(int userId)
		{
			using (var oAuth2ClientManager = _servicesMgr.CreateProxy<IOAuth2ClientManager>(ExecutionIdentity.System))
			{
				Services.Security.Models.OAuth2Client client;
				string clientName = GenerateClientName(userId);

				try
				{
					IEnumerable<Services.Security.Models.OAuth2Client> clients = oAuth2ClientManager.ReadAllAsync().ConfigureAwait(false).GetAwaiter().GetResult();
					client = clients.SingleOrDefault(x => x.Name.Equals(clientName, StringComparison.InvariantCulture));

					if (client == null)
					{
						client = oAuth2ClientManager.CreateAsync(clientName, OAuth2Flow.ClientCredentials,
							new List<Uri>(), userId).ConfigureAwait(false).GetAwaiter().GetResult();
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Error occured while getting OAuth2Client: {ex.Message}");
					throw new InvalidOperationException($"Failed to retrieve OAuth2Client for user with id: {userId}", ex);
				}

				_logger.LogInformation($"OAuth2Client for user with id: {userId} created successfully");
				return client;
			}
		}

		private string GenerateClientName(int userId)
		{
			return $"{_OAUTH2_CLIENT_NAME_PREFIX} {userId}";
		}
	}
}