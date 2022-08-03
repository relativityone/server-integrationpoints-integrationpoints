using System;
using System.Linq;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade
{
    internal static class LoginHelperFacadeFactoryDeprecated
    {
        /// <summary>
        ///  This method can be used to retrieve instance of <see cref="ILoginHelperFacade"/> when resolving it from IoC container is not possible
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static ILoginHelperFacade Create(IAPILog apiLog)
        {
            var retryHandlerFactory = new RetryHandlerFactory(apiLog);
            var instrumentationProvider = new ExternalServiceInstrumentationProviderWithoutJobContext(apiLog);

            var decorators = new Func<ILoginHelperFacade, ILoginHelperFacade>[]
            {
                authProvider => new LoginHelperInstrumentationDecorator(authProvider , instrumentationProvider),
                authProvider => new LoginHelperRetryDecorator(authProvider , retryHandlerFactory),
            };

            ILoginHelperFacade baseAuthProvider = new LoginHelperFacade();

            return decorators.Aggregate(
                baseAuthProvider,
                (authProvider, decorator) => decorator(authProvider)
            );
        }
    }
}
