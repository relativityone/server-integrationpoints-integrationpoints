using System;
using System.Security.Authentication;
using Relativity.API;
using Relativity.DataExchange;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Enumeration;
using kCura.WinEDDS.Exceptions;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.Properties;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class ImportApiFactory : IImportApiFactory
	{
		private readonly ISystemEventLoggingService _systemEventLoggingService;
		private readonly IAPILog _logger;
		private readonly IAuthTokenGenerator _authTokenGenerator;

		private class RelativityTokenProvider : IRelativityTokenProvider
		{
			private readonly IAuthTokenGenerator _authTokenGenerator;

			public RelativityTokenProvider(IAuthTokenGenerator authTokenGenerator)
			{
				_authTokenGenerator = authTokenGenerator;
			}

			public string GetToken()
			{
				return _authTokenGenerator.GetAuthToken();
			}
		}

		public ImportApiFactory(IAuthTokenGenerator authTokenGenerator, ISystemEventLoggingService systemEventLoggingService, IAPILog logger)
		{
			_authTokenGenerator = authTokenGenerator;
			_systemEventLoggingService = systemEventLoggingService;
			_logger = logger.ForContext<ImportApiFactory>();
		}

		public virtual IImportAPI GetImportAPI(ImportSettings settings)
		{
			LogImportSettings(settings);
			try
			{
				IImportAPI importApi = CreateImportAPIForSettings(settings);
				var concreteImplementation = (Relativity.ImportAPI.ImportAPI) importApi;
				concreteImplementation.ExecutionSource = ExecutionSourceEnum.RIP;
				LogImportApiCreated();
				return importApi;
			}
			catch (InvalidLoginException ex)
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
			catch (Exception ex)
			{
				LogCreatingImportApiError(ex, settings.WebServiceURL);
				throw;
			}
		}

		public IImportApiFacade GetImportApiFacade(ImportSettings settings)
		{
			return new ImportApiFacade(this, settings, _logger);
		}

		protected virtual IImportAPI CreateImportAPIForSettings(ImportSettings settings)
		{
			if (settings.FederatedInstanceArtifactId != null)
			{
				throw new NotSupportedException("Instance-to-instance import is not supported.");
			}

			return CreateImportAPI(settings.WebServiceURL);
		}

		protected virtual IImportAPI CreateImportAPI(string webServiceUrl)
		{
			IRelativityTokenProvider relativityTokenProvider = new RelativityTokenProvider(_authTokenGenerator);

			return ExtendedImportAPI.CreateByTokenProvider(webServiceUrl, relativityTokenProvider);
		}

		#region Logging

		private void LogImportSettings(ImportSettings importSettings)
		{
			var importSettingsForLogging = new ImportSettingsForLogging(importSettings);

			_logger.LogInformation("ImportSettings: {@importSettings}", importSettingsForLogging);
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
