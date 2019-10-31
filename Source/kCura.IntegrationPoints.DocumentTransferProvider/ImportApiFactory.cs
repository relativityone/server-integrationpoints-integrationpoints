using kCura.IntegrationPoints.Config;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	public class ImportApiFactory : IImportApiFactory
	{
		private readonly IWebApiConfig _webApiConfig;

		public ImportApiFactory(IWebApiConfig webApiConfig)
		{
			_webApiConfig = webApiConfig;
		}

		public IImportAPI Create()
		{
			return ImportAPI.CreateByRsaBearerToken(_webApiConfig.GetWebApiUrl);
		}
	}
}
