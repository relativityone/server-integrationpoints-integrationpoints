using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public abstract class ExporterEventsWrapper : IExporter
	{
		private readonly IExporterStatusNotification _exporterStatusNotification;

		protected ExporterEventsWrapper(IExporterStatusNotification exporterStatusNotification)
		{
			_exporterStatusNotification = exporterStatusNotification;
		}

		public event IExporterStatusNotification.FatalErrorEventEventHandler FatalErrorEvent
		{
			add { _exporterStatusNotification.FatalErrorEvent += value; }
			remove { _exporterStatusNotification.FatalErrorEvent -= value; }
		}

		public event IExporterStatusNotification.StatusMessageEventHandler StatusMessage

		{
			add { _exporterStatusNotification.StatusMessage += value; }
			remove { _exporterStatusNotification.StatusMessage -= value; }
		}

		public event IExporterStatusNotification.FileTransferModeChangeEventEventHandler FileTransferModeChangeEvent

		{
			add { _exporterStatusNotification.FileTransferModeChangeEvent += value; }
			remove { _exporterStatusNotification.FileTransferModeChangeEvent -= value; }
		}

		public abstract IUserNotification InteractionManager { get; set; }

		public abstract void Run();
	}
}