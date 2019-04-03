using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Provider
{
	public interface IProviderInstaller
	{
		Task<bool> InstallProvidersAsync(IEnumerable<IntegrationPoints.Contracts.SourceProvider> providersToInstall);
	}
}
