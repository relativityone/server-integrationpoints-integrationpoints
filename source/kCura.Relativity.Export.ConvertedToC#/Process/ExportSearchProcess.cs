using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using kCura.Relativity.Export.Exports;
using kCura.Relativity.Export.FileObjects;
using kCura.Relativity.Export.Types;

namespace kCura.Relativity.Export.Process
{

	public class ExportSearchProcess : kCura.Windows.Process.ProcessBase
	{

		public ExportFile ExportFile;
		private Exporter withEventsField__searchExporter;
		private Exporter _searchExporter {
			get { return withEventsField__searchExporter; }
			set {
				if (withEventsField__searchExporter != null) {
					withEventsField__searchExporter.FileTransferModeChangeEvent -= _searchExporter_FileTransferModeChangeEvent;
					withEventsField__searchExporter.StatusMessage -= _productionExporter_StatusMessage;
					withEventsField__searchExporter.FatalErrorEvent -= _productionExporter_FatalErrorEvent;
					withEventsField__searchExporter.ShutdownEvent -= _searchExporter_ShutdownEvent;
				}
				withEventsField__searchExporter = value;
				if (withEventsField__searchExporter != null) {
					withEventsField__searchExporter.FileTransferModeChangeEvent += _searchExporter_FileTransferModeChangeEvent;
					withEventsField__searchExporter.StatusMessage += _productionExporter_StatusMessage;
					withEventsField__searchExporter.FatalErrorEvent += _productionExporter_FatalErrorEvent;
					withEventsField__searchExporter.ShutdownEvent += _searchExporter_ShutdownEvent;
				}
			}
		}
		private System.DateTime _startTime;
		private Int32 _errorCount;
		private Int32 _warningCount;

		private string _uploadModeText = null;
		protected override void Execute()
		{
			_startTime = DateTime.Now;
			_warningCount = 0;
			_errorCount = 0;
			_searchExporter = new Exporter(this.ExportFile, this.ProcessController);

			if (!_searchExporter.ExportSearch()) {
				this.ProcessObserver.RaiseProcessCompleteEvent(false, _searchExporter.ErrorLogFileName);
			} else {
				this.ProcessObserver.RaiseStatusEvent("", "Export completed");
				this.ProcessObserver.RaiseProcessCompleteEvent();
			}
		}

		private void _searchExporter_FileTransferModeChangeEvent(string mode)
		{
			if (_uploadModeText == null) {
				_uploadModeText = Config.FileTransferModeExplanationText(false);
			}
			this.ProcessObserver.RaiseStatusBarEvent("File Transfer Mode: " + mode, _uploadModeText);
		}

		private void _productionExporter_StatusMessage(ExportEventArgs e)
		{
			switch (e.EventType) {
				case kCura.Windows.Process.EventType.Error:
					_errorCount += 1;
					this.ProcessObserver.RaiseErrorEvent(e.DocumentsExported.ToString(), e.Message);
					break;
				case kCura.Windows.Process.EventType.Progress:
					this.ProcessObserver.RaiseStatusEvent("", e.Message);
					break;
				case kCura.Windows.Process.EventType.Status:
					this.ProcessObserver.RaiseStatusEvent(e.DocumentsExported.ToString(), e.Message);
					break;
				case kCura.Windows.Process.EventType.Warning:
					_warningCount += 1;
					this.ProcessObserver.RaiseWarningEvent(e.DocumentsExported.ToString(), e.Message);
					break;
			}
			IDictionary statDict = null;
			if ((e.AdditionalInfo != null) && e.AdditionalInfo is IDictionary) {
				statDict = (IDictionary)e.AdditionalInfo;
			}

			this.ProcessObserver.RaiseProgressEvent(e.TotalDocuments, e.DocumentsExported, _warningCount, _errorCount, _startTime, new DateTime(), null, null, statDict);
		}

		private void _productionExporter_FatalErrorEvent(string message, System.Exception ex)
		{
			this.ProcessObserver.RaiseFatalExceptionEvent(ex);
		}

		private void _searchExporter_ShutdownEvent()
		{
			this.ProcessObserver.Shutdown();
		}
	}
}
