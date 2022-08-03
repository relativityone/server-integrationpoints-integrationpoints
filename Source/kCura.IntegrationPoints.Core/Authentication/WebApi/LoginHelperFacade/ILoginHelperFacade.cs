using System.Net;

namespace kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade
{
    internal interface ILoginHelperFacade
    {
        NetworkCredential LoginUsingAuthToken(string token, CookieContainer cookieContainer);
    }
}