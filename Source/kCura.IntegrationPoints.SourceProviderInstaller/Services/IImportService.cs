using System.Collections.Generic;

namespace kCura.IntegrationPoints.SourceProviderInstaller.Services
{
	internal interface IImportService
	{
		void InstallProviders(IEnumerable<SourceProvider> providers);
		void UninstallProvider(int applicationID);
	}
}
