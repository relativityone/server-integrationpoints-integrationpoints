using System;
using System.Linq;
using System.Security.Authentication;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class ImportApiFactory : IImportApiFactory
	{
		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";

		public virtual IExtendedImportAPI GetImportAPI(ImportSettings settings)
		{
			IExtendedImportAPI importAPI = null;
			try
			{
				if (RelativityVersion.IsRelativityVersion93OrGreater)
				{
					if (settings.RelativityUsername != null && settings.RelativityPassword != null)
					{
						importAPI = new ExtendedImportAPI(settings.RelativityUsername, settings.RelativityPassword, settings.WebServiceURL);
					}
					else
					{
						string username = _RELATIVITY_BEARER_USERNAME;
						string token = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;
						importAPI = new ExtendedImportAPI(username, token, settings.WebServiceURL);
					}
				
				}
				else
				{
					importAPI = new ExtendedImportAPI(settings.WebServiceURL);
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.Equals("Login failed."))
				{
					SystemEventLoggingService.WriteErrorEvent("Relativity Integration Points", "GetImportAPI", ex);
					throw new AuthenticationException(Properties.ErrorMessages.Login_Failed, ex);
				}
				//LoggedException.PreserveStack(ex);
				throw;
			}
			return importAPI;
		}
	}
}
