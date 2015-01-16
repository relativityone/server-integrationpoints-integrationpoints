using System.Collections.Generic;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	public interface IImportService
	{
		void InstallProviders(IEnumerable<SourceProvider> providers);
		void UninstallProvider();
	}
}
