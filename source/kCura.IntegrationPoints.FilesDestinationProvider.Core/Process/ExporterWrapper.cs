using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    public class ExporterWrapper : IExporter
    {
        private readonly Exporter _exporter;

        public ExporterWrapper(Exporter exporter)
        {
            _exporter = exporter;
        }

        public event IExporterStatusNotification.FatalErrorEventEventHandler FatalErrorEvent
        {
            add { _exporter.FatalErrorEvent += value; }
            remove { _exporter.FatalErrorEvent -= value; }
        }

        public event IExporterStatusNotification.StatusMessageEventHandler StatusMessage

        {
            add { _exporter.StatusMessage += value; }
            remove { _exporter.StatusMessage -= value; }
        }

        public event IExporterStatusNotification.FileTransferModeChangeEventEventHandler FileTransferModeChangeEvent

        {
            add { _exporter.FileTransferModeChangeEvent += value; }
            remove { _exporter.FileTransferModeChangeEvent -= value; }
        }

        public IUserNotification InteractionManager
        {
            get { return _exporter.InteractionManager; }
            set { _exporter.InteractionManager = value; }
        }

        public bool ExportSearch()
        {
            return _exporter.ExportSearch();
        }
    }
}