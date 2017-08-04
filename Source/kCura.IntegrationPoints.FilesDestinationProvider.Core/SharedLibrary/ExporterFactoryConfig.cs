using kCura.IntegrationPoints.Core.Managers;
using kCura.Windows.Process;
using kCura.WinEDDS;
using kCura.WinEDDS.Core.Export;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class ExporterFactoryConfig
	{
		public bool NameTextAndNativesAfterBegBates { get; set; }

		public IJobStopManager JobStopManager { get; set; }
		public IServiceFactory ServiceFactory { get; set; }
		public IFileNameProvider FileNameProvider { get; set; }

		public ExportFileFormatterFactory LoadFileFormatterFactory { get; set; }
		public Controller Controller { get; set; }

		public IExportConfig ExportConfig { get; set; }
	}
}