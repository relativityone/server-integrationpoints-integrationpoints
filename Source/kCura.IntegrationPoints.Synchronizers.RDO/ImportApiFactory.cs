using System;
using System.Linq;
using System.Security.Authentication;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.Model.Serializer;
using kCura.IntegrationPoints.Synchronizers.RDO.Properties;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Enumeration;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class ImportApiFactory : IImportApiFactory
	{
		protected readonly ITokenProvider _externalTokenProvider;
		protected readonly IFederatedInstanceManager _federatedInstanceManager;
		protected readonly ISystemEventLoggingService _systemEventLoggingService;
		protected readonly IAPILog _logger;
		protected readonly ISerializer _serializer;
	    protected readonly IAuthTokenGenerator _authTokenGenerator; 

		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";

		public ImportApiFactory(ITokenProvider externalTokenProvider, IAuthTokenGenerator authTokenGenerator, IFederatedInstanceManager federatedInstanceManager, IHelper helper, ISystemEventLoggingService systemEventLoggingService, ISerializer serializer)
		{
			_externalTokenProvider = externalTokenProvider;
		    _authTokenGenerator = authTokenGenerator;
			_federatedInstanceManager = federatedInstanceManager;
			_systemEventLoggingService = systemEventLoggingService;
			_serializer = serializer;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ImportApiFactory>();
		}

		public virtual IExtendedImportAPI GetImportAPI(ImportSettings settings)
		{
			LogImportSettings(settings);
			IExtendedImportAPI importApi;
			try
            {
                importApi = CreateExtendedImportAPIForSettings(settings);
                // ExtendedImportAPI extends ImportAPI so the following cast is acceptable
                Relativity.ImportAPI.ImportAPI concreteImplementation = (Relativity.ImportAPI.ImportAPI)importApi;
                concreteImplementation.ExecutionSource = ExecutionSourceEnum.RIP;
            }
            catch (Exception ex)
			{
				if (string.Equals(ex.Message, "Login failed."))
				{
					ThrowAuthenticationException(settings, ex);
				}
				LogCreatingImportApiError(ex, settings.WebServiceURL);
				throw;
			}
			LogImportApiCreated();
			return importApi;
		}

        protected IExtendedImportAPI CreateExtendedImportAPIForSettings(ImportSettings settings)
        {
            string username;
            string webServiceUrl;
            string token;

            if (RelativityCredentialsProvided(settings))
            {
                LogCreatingImportApiWithPassword(settings.WebServiceURL);
                username = settings.RelativityUsername;
                token = settings.RelativityPassword;
                webServiceUrl = settings.WebServiceURL;
            }
            else
            {
                LogCreatingImportApiWithToken(settings.WebServiceURL);
                username = _RELATIVITY_BEARER_USERNAME;
                webServiceUrl = GetWebServiceUrl(settings);
                token = GetToken(settings);
            }

            return CreateExtendedImportAPI(username, token, webServiceUrl);
        }

	    protected virtual IExtendedImportAPI CreateExtendedImportAPI(string username, string token, string webServiceUrl)
        {
            return new ExtendedImportAPI(username, token, webServiceUrl);
        }

	    private bool RelativityCredentialsProvided(ImportSettings settings)
	    {
	        return settings.RelativityUsername != null && settings.RelativityPassword != null;
	    }

	    private string GetToken(ImportSettings settings)
		{
			if (settings.FederatedInstanceArtifactId == null)
			{
			    return _authTokenGenerator.GetAuthToken();
			}

			OAuthClientDto oAuthClientDto = _serializer.Deserialize<OAuthClientDto>(settings.FederatedInstanceCredentials);
			FederatedInstanceDto federatedInstance =
				_federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(settings.FederatedInstanceArtifactId.Value);

			return _externalTokenProvider.GetExternalSystemToken(oAuthClientDto.ClientId, oAuthClientDto.ClientSecret,
				new Uri(federatedInstance.InstanceUrl));
		}

	    private string GetWebServiceUrl(ImportSettings settings)
		{
			if (settings.FederatedInstanceArtifactId == null)
			{
				return settings.WebServiceURL;
			}

			FederatedInstanceDto federatedInstance =
				_federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(settings.FederatedInstanceArtifactId.Value);

			return federatedInstance.WebApiUrl;
		}

		private void ThrowAuthenticationException(ImportSettings settings, Exception ex)
		{
			LogLoginFailed(ex, settings.WebServiceURL);
			_systemEventLoggingService.WriteErrorEvent("Relativity Integration Points", "GetImportAPI", ex);
			var authException = new AuthenticationException(ErrorMessages.Login_Failed, ex);
			throw new IntegrationPointsException(ErrorMessages.Login_Failed, authException)
			{
				ShouldAddToErrorsTab = true,
				ExceptionSource = IntegrationPointsExceptionSource.IAPI
			};
		}

		#region Logging

		private void LogImportSettings(ImportSettings importSettings)
		{
			var serializer = new ImportSettingsForLogSerializer();
			var serializedSettings = serializer.Serialize(importSettings);
			_logger.LogInformation("ImportSettings: {serializedSettings}", serializedSettings);
		}

		private void LogCreatingImportApiWithPassword(string url)
		{
			_logger.LogDebug("Attempting to create ExtendedImportAPI ({URL}) using username and password for Relativity 9.3 or greater.", url);
		}

		private void LogCreatingImportApiWithToken(string url)
		{
			_logger.LogDebug("Attempting to create ExtendedImportAPI ({URL}) using token for Relativity 9.3 or greater.", url);
		}

		private void LogCreatingImportApiForOldRelativity(string url)
		{
			_logger.LogDebug("Attempting to create ExtendedImportAPI ({URL}) for old Relativity using only WebServiceURL.", url);
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
			_logger.LogDebug("Successfully created Import API.");
		}

		#endregion
	}
}
