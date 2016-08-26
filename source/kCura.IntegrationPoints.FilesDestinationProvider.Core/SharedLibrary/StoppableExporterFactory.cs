using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.Windows.Process;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	internal class StoppableExporterFactory : IExporterFactory
	{
		private readonly JobHistoryErrorServiceProvider _jobHistoryErrorServiceProvider;

		public StoppableExporterFactory(JobHistoryErrorServiceProvider jobHistoryErrorServiceProvider)
		{
			_jobHistoryErrorServiceProvider = jobHistoryErrorServiceProvider;
		}

		public IExporter Create(ExportFile exportFile)
		{
			var jobStopManager = _jobHistoryErrorServiceProvider?.JobHistoryErrorService.JobStopManager;
			var controller = new Controller();
			var exporter = new Exporter(exportFile, controller);
			var exporterWrapper = new SimpleExporter(exporter);
			if (jobStopManager == null)
			{
				return exporterWrapper;
			}
			return new StoppableExporter(exporterWrapper, controller, jobStopManager);
		}
	}
}