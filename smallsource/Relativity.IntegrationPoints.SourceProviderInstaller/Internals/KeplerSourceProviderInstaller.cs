using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals.Converters;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Internals
{
	internal class KeplerSourceProviderInstaller : ISourceProviderInstaller
	{
		private readonly IKeplerRequestHelper _keplerRetryHelper;

		public KeplerSourceProviderInstaller(IKeplerRequestHelper keplerRetryHelper)
		{
			_keplerRetryHelper = keplerRetryHelper;
		}

		public async Task InstallSourceProvidersAsync(int workspaceID, IEnumerable<SourceProvider> sourceProviders)
		{
			ValidateParameterIsNotNull(sourceProviders, nameof(sourceProviders));

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

		private void ValidateParameterIsNotNull(object parameterValue, string parameterName)
		{
			if (parameterValue == null)
			{
				throw new ArgumentNullException(parameterName);
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
