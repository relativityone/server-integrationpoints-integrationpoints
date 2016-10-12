using System;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Synchronizers.RDO.Properties;
using kCura.Relativity.ImportAPI;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class ImportApiFactory : IImportApiFactory
	{
		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";

		private readonly IAPILog _logger;

		public ImportApiFactory(IHelper helper)
		{
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
						string token = ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;
						importApi = new ExtendedImportAPI(username, token, settings.WebServiceURL);
					}
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
					SystemEventLoggingService.WriteErrorEvent("Relativity Integration Points", "GetImportAPI", ex);
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