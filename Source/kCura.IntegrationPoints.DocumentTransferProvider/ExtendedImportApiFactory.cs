using System.Linq;
using kCura.IntegrationPoints.Config;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	public class ExtendedImportApiFactory : IExtendedImportApiFactory
	{

		private const string _RELATIVITY_SPECIAL_USERNAME = "XxX_BearerTokenCredentials_XxX";
		private readonly IWebApiConfig _webApiConfig;

		public ExtendedImportApiFactory(IWebApiConfig webApiConfig)
		{
			_webApiConfig = webApiConfig;
		}

		public IExtendedImportAPI Create()
		{
			string authToken = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;
			return new ExtendedImportAPI(_RELATIVITY_SPECIAL_USERNAME, authToken, _webApiConfig.GetWebApiUrl);
		}
	}
}
