using System;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.Properties;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Enumeration;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class ImportApiFactory : IImportApiFactory
	{
		private readonly ITokenProvider _tokenProvider;
		private readonly IOAuthClientManager _oAuthClientManager;
		private readonly IFederatedInstanceManager _federatedInstanceManager;
		private readonly ISystemEventLoggingService _systemEventLoggingService;
		private readonly IAPILog _logger;

		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";

		public ImportApiFactory(
			ITokenProvider tokenProvider, 
			IOAuthClientManager oAuthClientManager, 
			IFederatedInstanceManager federatedInstanceManager,
			IHelper helper, ISystemEventLoggingService systemEventLoggingService)
		{
			_tokenProvider = tokenProvider;
			_oAuthClientManager = oAuthClientManager;
			_federatedInstanceManager = federatedInstanceManager;
			_systemEventLoggingService = systemEventLoggingService;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ImportApiFactory>();
		}

		public virtual IExtendedImportAPI GetImportAPI(ImportSettings settings)
		{
			IExtendedImportAPI importApi;
			try
			{
				if (RelativityVersion.IsRelativityVersion93OrGreater)
				{
					if ((settings.RelativityUsername != null) && (settings.RelativityPassword != null))
					{
						LogCreatingImportApiWithPassword(settings.WebServiceURL);
						importApi = new ExtendedImportAPI(settings.RelativityUsername, settings.RelativityPassword, settings.WebServiceURL);
					}
					else
					{
						LogCreatingImportApiWithToken(settings.WebServiceURL);
						string username = _RELATIVITY_BEARER_USERNAME;
						string webServiceUrl;
						string token;

						if (settings.FederatedInstanceArtifactId != null)
						{
							OAuthClientDto oAuthClientDto =
								_oAuthClientManager.RetrieveOAuthClientForFederatedInstance(settings.FederatedInstanceArtifactId.Value);
							FederatedInstanceDto federatedInstance =
								_federatedInstanceManager.RetrieveFederatedInstance(settings.FederatedInstanceArtifactId.Value);

							token = _tokenProvider.GetExternalSystemToken(oAuthClientDto.ClientId, oAuthClientDto.ClientSecret,
								new Uri(federatedInstance.InstanceUrl));
							webServiceUrl = federatedInstance.WebApiUrl;
						}
						else
						{
							token = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;
							webServiceUrl = settings.WebServiceURL;
						}
						importApi = new ExtendedImportAPI(username, token, webServiceUrl);
					}
					// ExtendedImportAPI extends ImportAPI so the following cast is acceptable
					Relativity.ImportAPI.ImportAPI concreteImplementation = (Relativity.ImportAPI.ImportAPI)importApi;
					concreteImplementation.ExecutionSource = ExecutionSourceEnum.RIP;
				}
				else
				{
					LogCreatingImportApiForOldRelativity(settings.WebServiceURL);
					importApi = new ExtendedImportAPI(settings.WebServiceURL);
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.Equals("Login failed."))
				{
					LogLoginFailed(ex, settings.WebServiceURL);
					_systemEventLoggingService.WriteErrorEvent("Relativity Integration Points", "GetImportAPI", ex);
					throw new AuthenticationException(ErrorMessages.Login_Failed, ex);
				}
				//LoggedException.PreserveStack(ex);
				LogCreatingImportApiError(ex, settings.WebServiceURL);
				throw;
			}
			LogImportApiCreated();
			return importApi;
		}

		#region Logging

		private void LogCreatingImportApiWithPassword(string url)
		{
			_logger.LogInformation("Attempting to create ExtendedImportAPI ({URL}) using username and password for Relativity 9.3 or greater.", url);
		}

		private void LogCreatingImportApiWithToken(string url)
		{
			_logger.LogInformation("Attempting to create ExtendedImportAPI ({URL}) using token for Relativity 9.3 or greater.", url);
		}

		private void LogCreatingImportApiForOldRelativity(string url)
		{
			_logger.LogInformation("Attempting to create ExtendedImportAPI ({URL}) for old Relativity using only WebServiceURL.", url);
		}

		private void LogCreatingImportApiError(Exception ex, string url)
		{
			_logger.LogError(ex, "Failed to create Import API ({URL}).", url);
		}

		private void LogLoginFailed(Exception ex, string url)
		{
			_logger.LogError(ex, "Failed to create Import API ({URL}): Login failed.", url);
		}

		private void LogImportApiCreated()
		{
			_logger.LogInformation("Successfully created Import API.");
		}

		#endregion
	}
}
