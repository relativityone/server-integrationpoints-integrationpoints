using Relativity.DataExchange;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
    public class ImportApiFactory : IImportApiFactory
    {
        private readonly IWebApiConfig _webApiConfig;
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

        public ImportApiFactory(IWebApiConfig webApiConfig, IAuthTokenGenerator authTokenGenerator)
        {
            _webApiConfig = webApiConfig;
            _authTokenGenerator = authTokenGenerator;
        }

        public IImportAPI Create()
        {
            IRelativityTokenProvider relativityTokenProvider = new RelativityTokenProvider(_authTokenGenerator);

            return ExtendedImportAPI.CreateByTokenProvider(_webApiConfig.GetWebApiUrl, relativityTokenProvider);
        }
    }
}
