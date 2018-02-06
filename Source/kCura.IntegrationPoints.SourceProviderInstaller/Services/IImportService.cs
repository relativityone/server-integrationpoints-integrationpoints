using System.Collections.Generic;

namespace kCura.IntegrationPoints.SourceProviderInstaller.Services
{
	internal interface IImportService
	{
		void InstallProviders(IList<SourceProvider> providers);
		void UninstallProviders(int applicationID);
	}
}
