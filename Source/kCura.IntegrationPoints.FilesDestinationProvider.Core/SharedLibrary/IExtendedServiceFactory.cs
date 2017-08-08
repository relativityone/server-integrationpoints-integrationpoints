using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public interface IExtendedServiceFactory : IServiceFactory
	{
		ICaseManager CreateCaseManager();
	}
}