namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public interface IExportServiceFactory
	{
		IExtendedServiceFactory Create(ExportDataContext exportDataContext);
	}
}