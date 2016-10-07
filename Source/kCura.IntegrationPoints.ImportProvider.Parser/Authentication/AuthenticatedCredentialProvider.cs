using System.Net;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.ImportProvider.Parser.Authentication.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Authentication
{
    public class AuthenticatedCredentialProvider : IAuthenticatedCredentialProvider
    {
        ICredentialProvider _credentialProvider;

        public AuthenticatedCredentialProvider(ICredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
        }

        public NetworkCredential GetAuthenticatedCredential()
        {
            WinEDDS.Config.WebServiceURL = (new WebApiConfig()).GetWebApiUrl;
            return _credentialProvider.Authenticate(new System.Net.CookieContainer());
        }
    }
}
