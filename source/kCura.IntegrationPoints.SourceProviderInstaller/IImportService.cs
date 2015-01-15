namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	public interface IImportService
	{
		void InstallProvider(SourceProvider provider);
		void UninstallProvider();
	}
}
