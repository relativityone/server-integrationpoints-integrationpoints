using kCura.IntegrationPoints.Domain.Managers;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.DataExchange.Export;
using Relativity.DataExchange.Process;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class ExporterFactoryConfig
	{
		public bool NameTextAndNativesAfterBegBates { get; set; }

		public IJobStopManager JobStopManager { get; set; }
		public IServiceFactory ServiceFactory { get; set; }
		public IFileNameProvider FileNameProvider { get; set; }

		public ExportFileFormatterFactory LoadFileFormatterFactory { get; set; }
		public ProcessContext Controller { get; set; }

		public IExportConfig ExportConfig { get; set; }
	}
}