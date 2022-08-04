using System;
using System.Linq;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation
{
    public static class SecretStoreFacadeFactory_Deprecated
    {
        public static ISecretStoreFacade Create(Func<ISecretStore> secretStoreFunc, IAPILog apiLog)
        {
            var retryHandlerFactory = new RetryHandlerFactory(apiLog);
            var instrumentationProvider = new ExternalServiceInstrumentationProviderWithoutJobContext(apiLog);

            var decorators = new Func<ISecretStoreFacade, ISecretStoreFacade>[]
            {
                ss => new SecretStoreFacadeInstrumentationDecorator(ss , instrumentationProvider),
                ss => new SecretStoreFacadeRetryDecorator(ss , retryHandlerFactory),
            };

            var secretStoreLazy = new Lazy<ISecretStore>(secretStoreFunc);
            ISecretStoreFacade secretStoreFacade = new SecretStoreFacade(secretStoreLazy);

            return decorators.Aggregate(
                secretStoreFacade,
                (secretStore, decorator) => decorator(secretStore)
            );
        }
    }
}
