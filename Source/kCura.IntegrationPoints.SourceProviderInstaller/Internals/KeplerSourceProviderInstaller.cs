using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Services;
using kCura.IntegrationPoints.SourceProviderInstaller.Internals.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.SourceProviderInstaller.Internals
{
    internal class KeplerSourceProviderInstaller : ISourceProviderInstaller
    {
        private readonly IKeplerRequestHelper _keplerRetryHelper;

        public KeplerSourceProviderInstaller(IKeplerRequestHelper keplerRetryHelper)
        {
            _keplerRetryHelper = keplerRetryHelper;
        }

        public async Task InstallSourceProviders(int workspaceID, IEnumerable<SourceProvider> sourceProviders)
        {
            var request = new InstallProviderRequest
            {
                WorkspaceID = workspaceID,
                ProvidersToInstall = sourceProviders.Select(x => x.ToInstallProviderDto()).ToList()
            };

            InstallProviderResponse response = await SendInstallProviderRequestWithRetriesAsync(request).ConfigureAwait(false);
            if (!response.Success)
            {
                throw new InvalidSourceProviderException($"An error occured while installing source providers: {response.ErrorMessage}");
            }
        }
        
        private Task<InstallProviderResponse> SendInstallProviderRequestWithRetriesAsync(InstallProviderRequest request)
        {
            return _keplerRetryHelper.ExecuteWithRetriesAsync<IProviderManager, InstallProviderRequest, InstallProviderResponse>(
                (providerManager, r) => providerManager.InstallProviderAsync(r),
                request
            );
        }
    }
}
