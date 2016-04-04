
using System;

namespace kCura.Windows.Process
{
	public class Controller
	{
		public event HaltProcessEventEventHandler HaltProcessEvent;
		public delegate void HaltProcessEventEventHandler(Guid processID);
		public event ExportServerErrorsEventEventHandler ExportServerErrorsEvent;
		public delegate void ExportServerErrorsEventEventHandler(string exportLocation);
		public event ExportErrorReportEventEventHandler ExportErrorReportEvent;
		public delegate void ExportErrorReportEventEventHandler(string exportLocation);
		public event ExportErrorFileEventEventHandler ExportErrorFileEvent;
		public delegate void ExportErrorFileEventEventHandler(string exportLocation);
		public event ParentFormClosingEventEventHandler ParentFormClosingEvent;
		public delegate void ParentFormClosingEventEventHandler(Guid processID);

		public void HaltProcess(Guid processID)
		{
			if (HaltProcessEvent != null) {
				HaltProcessEvent(processID);
			}
		}

		public void ExportServerErrors(string exportLocation)
		{
			if (ExportServerErrorsEvent != null) {
				ExportServerErrorsEvent(exportLocation);
			}
		}

		public void ExportErrorReport(string exportLocation)
		{
			if (ExportErrorReportEvent != null) {
				ExportErrorReportEvent(exportLocation);
			}
		}

		public void ExportErrorFile(string exportLocation)
		{
			if (ExportErrorFileEvent != null) {
				ExportErrorFileEvent(exportLocation);
			}
		}

		public void ParentFormClosing(Guid processID)
		{
			if (ParentFormClosingEvent != null) {
				ParentFormClosingEvent(processID);
			}
		}

	}
}
