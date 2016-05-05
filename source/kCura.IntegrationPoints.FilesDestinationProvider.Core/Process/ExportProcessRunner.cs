
namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public class ExportProcessRunner
	{
		public void StartWith(ExportSettings settings)
		{
			WinEDDS.Exporter searchExporter = new ExportProcessBuilder().Create(settings);
			searchExporter.ExportSearch();
		}
	}
}
