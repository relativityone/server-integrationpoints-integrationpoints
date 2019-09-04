using System;
using System.Security.Authentication;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Synchronizers.RDO.Properties;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Enumeration;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class ImportApiFactory : IImportApiFactory
	{
		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";

		protected readonly ISystemEventLoggingService _systemEventLoggingService;
		protected readonly IAPILog _logger;
		protected readonly IAuthTokenGenerator _authTokenGenerator;

		public ImportApiFactory(
			IAuthTokenGenerator authTokenGenerator,
			IAPILog logger,
			ISystemEventLoggingService systemEventLoggingService)
		{
			_authTokenGenerator = authTokenGenerator;
			_systemEventLoggingService = systemEventLoggingService;
			_logger = logger.ForContext<ImportApiFactory>();
		}

		public virtual IExtendedImportAPI GetImportAPI(ImportSettings settings)
		{
			LogImportSettings(settings);
			IExtendedImportAPI importApi;
			try
			{
				importApi = CreateExtendedImportAPIForSettings(settings);
				// ExtendedImportAPI extends ImportAPI so the following cast is acceptable
				var concreteImplementation = (Relativity.ImportAPI.ImportAPI)importApi;
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
			if (settings.FederatedInstanceArtifactId != null)
			{
				throw new NotSupportedException("i2i is not supported");
			}

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
				webServiceUrl = settings.WebServiceURL;

				token = _authTokenGenerator.GetAuthToken();
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
			_logger.LogInformation("ImportSettings: {@importSettings}", importSettings);
		}

		private void LogCreatingImportApiWithPassword(string url)
		{
			_logger.LogDebug("Attempting to create ExtendedImportAPI ({URL}) using username and password for Relativity 9.3 or greater.", url);
		}

		private void LogCreatingImportApiWithToken(string url)
		{
			_logger.LogDebug("Attempting to create ExtendedImportAPI ({URL}) using token for Relativity 9.3 or greater.", url);
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
