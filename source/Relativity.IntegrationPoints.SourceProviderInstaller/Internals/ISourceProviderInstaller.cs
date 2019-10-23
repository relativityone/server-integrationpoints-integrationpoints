using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Contracts;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Internals
{
	internal interface ISourceProviderInstaller
	{
		Task InstallSourceProvidersAsync(int workspaceID, IEnumerable<SourceProvider> sourceProviders);
	}
}
