﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;

namespace kCura.IntegrationPoints.Core.Authentication
{
	public class OAuth2ClientFactory : IOAuth2ClientFactory
	{
		private readonly IRetryHandler _retryHandler;
		private readonly IHelper _helper;
		private readonly IAPILog _logger;

		public OAuth2ClientFactory(IRetryHandlerFactory retryHandlerFactory, IHelper helper)
		{
			const int maxNumberOfRetries = 4;
			_retryHandler = retryHandlerFactory.Create(maxNumberOfRetries);
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<OAuth2ClientFactory>();
		}

		public OAuth2Client GetOauth2Client(int userId)
		{
			string clientName = GenerateClientName(userId);

			try
			{
				OAuth2Client client = _retryHandler.ExecuteWithRetries(() =>
				{
					using (var oAuth2ClientManager = _helper.GetServicesManager().CreateProxy<IOAuth2ClientManager>(ExecutionIdentity.System))
					{
						IEnumerable<OAuth2Client> clients = oAuth2ClientManager.ReadAllAsync().ConfigureAwait(false).GetAwaiter().GetResult();
						OAuth2Client newOAuth2Client = clients.SingleOrDefault(x => x.Name.Equals(clientName));

						if (newOAuth2Client == null)
						{
							newOAuth2Client = oAuth2ClientManager.CreateAsync(clientName, OAuth2Flow.ClientCredentials,
								new List<Uri>(), userId).ConfigureAwait(false).GetAwaiter().GetResult();
						}

						return newOAuth2Client;
					}
				});

				LogGetOAuth2ClientSuccess(userId);
				return client;
			}
			catch (Exception ex)
			{
				LogGetOAuth2ClientError(ex);
				throw new InvalidOperationException($"Failed to retrieve OAuth2Client for user with id: {userId}", ex);
			}
		}

		private string GenerateClientName(int userId)
		{
			return $"{Constants.IntegrationPoints.OAUTH2_CLIENT_NAME_PREFIX} {userId}";
		}

		private void LogGetOAuth2ClientError(Exception ex)
		{
			_logger.LogError(ex, "Error occured while getting OAuth2Client: {message}", ex.Message);
		}

		private void LogGetOAuth2ClientSuccess(int userId)
		{
			_logger.LogInformation("OAuth2Client for user with id: {userId} created successfully", userId);
		}
	}
}