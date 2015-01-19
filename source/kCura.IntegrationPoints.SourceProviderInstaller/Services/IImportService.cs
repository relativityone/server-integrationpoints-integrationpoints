using System.Collections.Generic;

namespace kCura.IntegrationPoints.SourceProviderInstaller.Services
{
	public interface IImportService
	{
		void InstallProviders(IEnumerable<SourceProvider> providers);
		void UninstallProvider(int applicationID);
	}
}
