using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using System;
using System.Runtime.InteropServices;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Guid("AE453CBD-2B18-4EEE-A66C-A92E1A91C641")]
    [EventHandler.CustomAttributes.Description("This is an event handler to register back destination providers after creating workspace using the template that has integration point installed.")]
    public sealed class DestinationProvidersMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
    {
        protected override string SuccessMessage => "RIP Destination Providers registered successfully";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Error occured while installing destination providers";
        }

        protected override void Run()
        {
            IRdoSynchronizerProvider synchronizerProvider = CreateSynchronizerProvider();

            synchronizerProvider.CreateOrUpdateDestinationProviders();
        }

        private IRdoSynchronizerProvider CreateSynchronizerProvider()
        {
            IRelativityObjectManager objectManager = CreateObjectManager();
            var destinationProviderRepository = new DestinationProviderRepository(Logger, objectManager);
            return new RdoSynchronizerProvider(destinationProviderRepository, Logger);
        }

        private IRelativityObjectManager CreateObjectManager()
        {
            var factory = new RelativityObjectManagerFactory(Helper);
            return factory.CreateRelativityObjectManager(Helper.GetActiveCaseID());
        }
    }
}
