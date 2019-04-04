using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Services;
using kCura.IntegrationPoints.SourceProviderInstaller.Internals.Converters;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.SourceProviderInstaller.Internals
{
    internal class KeplerSourceProviderInstaller
    {
        private const int _SEND_INSTALL_REQUEST_MAX_RETRIES_NUMBER = 3;
        private const int _SEND_INSTALL_REQUEST_DELAY_BETWEEN_RETRIES_IN_MS = 3000;

        private readonly IAPILog _logger;
        private readonly IServicesMgr _servicesManager;

        public KeplerSourceProviderInstaller(IAPILog logger, IServicesMgr servicesManager)
        {
            _servicesManager = servicesManager;
            _logger = logger?.ForContext<KeplerSourceProviderInstaller>();
        }

        public async Task InstallSourceProviders(int workspaceID, IEnumerable<SourceProvider> sourceProviders)
        {
            var request = new InstallProviderRequest
            {
                WorkspaceID = workspaceID,
                ProvidersToInstall = sourceProviders.Select(x => x.ToProviderToInstallDto()).ToList()
            };

            InstallProviderResponse response = await SendInstallProviderRequestWithRetriesAsync(request);
            if (!response.Success)
            {
                throw new InvalidSourceProviderException($"An error occured while installing source providers: {response.ErrorMessage}");
            }
        }

        /// <summary>
        /// We cannot use Polly, because it would require adding external dependency to our SDK
        /// </summary>
        private async Task<InstallProviderResponse> SendInstallProviderRequestWithRetriesAsync(InstallProviderRequest request, int attemptNumber = 1)
        {
            try
            {
                using (var providerManager = _servicesManager.CreateProxy<IProviderManager>(ExecutionIdentity.CurrentUser))
                {
                    return await providerManager.InstallProviderAsync(request).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Installing provider failed, attempt {attemptNumber} out of {numberOfRetries}.",
                    attemptNumber,
                    _SEND_INSTALL_REQUEST_MAX_RETRIES_NUMBER
                );
                if (attemptNumber > _SEND_INSTALL_REQUEST_MAX_RETRIES_NUMBER)
                {
                    throw new InvalidSourceProviderException($"Error occured while sending request to {nameof(IProviderManager.InstallProviderAsync)}");
                }
            }

            await Task.Delay(_SEND_INSTALL_REQUEST_DELAY_BETWEEN_RETRIES_IN_MS).ConfigureAwait(false);
            return await SendInstallProviderRequestWithRetriesAsync(request, attemptNumber + 1).ConfigureAwait(false);
        }
    }
}
