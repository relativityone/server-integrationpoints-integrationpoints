using System;
using System.Net;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade;
using kCura.IntegrationPoints.Domain.Authentication;

namespace kCura.IntegrationPoints.Core.Authentication.WebApi
{
    internal class WebApiLoginService : IWebApiLoginService
    {
        private readonly ILoginHelperFacade _authProvider;
        private readonly IAuthTokenGenerator _tokenGenerator;
        private readonly ILogger<WebApiLoginService> _logger;

        public WebApiLoginService(
            ILoginHelperFacade authProvider,
            IAuthTokenGenerator tokenGenerator,
            ILogger<WebApiLoginService> logger)
        {
            _authProvider = authProvider;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
        }

        public NetworkCredential Authenticate(CookieContainer cookieContainer)
        {
            try
            {
                string token = _tokenGenerator.GetAuthToken();
                return _authProvider.LoginUsingAuthToken(token, cookieContainer);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while authenticating user. Details: {e.Message}");
                throw;
            }
        }
    }
}
