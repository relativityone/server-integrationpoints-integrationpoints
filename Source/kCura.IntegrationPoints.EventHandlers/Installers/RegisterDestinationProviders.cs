using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using System;
using System.Runtime.InteropServices;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [EventHandler.CustomAttributes.Description("Register RIP Destination Providers")]
    [EventHandler.CustomAttributes.RunOnce(true)]
    [Guid("4E058CF3-9C3E-41AA-8D3D-2CA8F1E06E08")]
    public class RegisterDestinationProviders : PostInstallEventHandlerBase
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
            var destinationProviderRepository = new DestinationProviderRepository(Logger, ObjectManager);
            return new RdoSynchronizerProvider(destinationProviderRepository, Logger);
        }
    }
}
