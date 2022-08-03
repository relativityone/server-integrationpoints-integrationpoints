using System.Net;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;

namespace kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade
{
    internal class LoginHelperRetryDecorator : ILoginHelperFacade
    {
        private const ushort _MAX_NUMBER_OF_RETRIES = 3;
        private const ushort _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC = 3;

        private readonly ILoginHelperFacade _authProvider;
        private readonly IRetryHandler _retryHandler;

        public LoginHelperRetryDecorator(ILoginHelperFacade authProvider, IRetryHandlerFactory retryHandlerFactory)
        {
            _authProvider = authProvider;
            _retryHandler = retryHandlerFactory.Create(
                _MAX_NUMBER_OF_RETRIES,
                _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC);
        }

        public NetworkCredential LoginUsingAuthToken(string token, CookieContainer cookieContainer)
        {
            return _retryHandler.ExecuteWithRetries(
                () => _authProvider.LoginUsingAuthToken(token, cookieContainer)
            );
        }
    }
}
