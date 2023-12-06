using System;
using System.Runtime.InteropServices;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data.Repositories.Implementations;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [EventHandler.CustomAttributes.Description("Register RIP Destination Providers")]
    [EventHandler.CustomAttributes.RunOnce(true)]
    [Guid("4E058CF3-9C3E-41AA-8D3D-2CA8F1E06E08")]
    public class RegisterDestinationProviders : PostInstallEventHandlerBase
    {
        protected override string SuccessMessage => "RIP Destination Providers registered successfully";

        private readonly object _executionLock = new object();
        private int _executionAttempt;

        protected override string GetFailureMessage(Exception ex)
        {
            return "Error occured while installing destination providers";
        }

        protected override void Run()
        {
            lock (_executionLock)
            {
                _executionAttempt++;
                if (_executionAttempt == 1)
                {
                    IRdoSynchronizerProvider synchronizerProvider = CreateSynchronizerProvider();
                    synchronizerProvider.CreateOrUpdateDestinationProviders();
                }
                else
                {
                    Logger.LogError(
                        "Unwanted execution of {eventHandlerName} Event Handler detected - attempt {attempt}",
                        nameof(RegisterDestinationProviders),
                        _executionAttempt);
                }
            }
        }

        private IRdoSynchronizerProvider CreateSynchronizerProvider()
        {
            var destinationProviderRepository = new DestinationProviderRepository(Logger, ObjectManager);
            return new RdoSynchronizerProvider(destinationProviderRepository, Logger);
        }
    }
}
