using System.Net;

namespace kCura.IntegrationPoints.Core.Authentication.WebApi
{
    public interface IWebApiLoginService
    {
        NetworkCredential Authenticate(CookieContainer cookieContainer);
    }
}
