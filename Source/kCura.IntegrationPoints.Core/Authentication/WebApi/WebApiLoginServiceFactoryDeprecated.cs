using kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade;
using kCura.IntegrationPoints.Domain.Authentication;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Authentication.WebApi
{
    public static class WebApiLoginServiceFactoryDeprecated
    {
        /// <summary>
        ///  This method can be used to retrieve instance of <see cref="IWebApiLoginService"/> when resolving it from IoC container is not possible
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IWebApiLoginService Create(IAPILog logger)
        {
            ILoginHelperFacade webApiLoginHelper = LoginHelperFacadeFactoryDeprecated.Create(logger);
            var tokenGenerator = new ClaimsTokenGenerator();
            return new WebApiLoginService(webApiLoginHelper, tokenGenerator, logger);
        }
    }
}
