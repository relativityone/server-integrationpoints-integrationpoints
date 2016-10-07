using System.Net;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Authentication.Interfaces
{
    public interface IAuthenticatedCredentialProvider
    {
        NetworkCredential GetAuthenticatedCredential();
    }
}
