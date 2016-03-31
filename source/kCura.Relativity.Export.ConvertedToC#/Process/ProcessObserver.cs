using System;
using System.Collections;

namespace kCura.Windows.Process
{
	public class ProcessObserver
	{

		#region "Members"

		private string _tempFileName = "";
		private string _errorsFileName;
		private bool _inSafeMode;
		private System.IO.StreamWriter _outputWriter;
		private System.IO.StreamWriter _errorsWriter;

		private object _inputArgs;
		public bool InSafeMode {
			get { return _inSafeMode; }
			set { _inSafeMode = value; }
		}
		public object InputArgs {
			get { return _inputArgs; }
			set { _inputArgs = value; }
		}

		#endregion

		#region "Events"

		public event OnProcessFatalExceptionEventHandler OnProcessFatalException;
		public delegate void OnProcessFatalExceptionEventHandler(Exception ex);
		public event OnProcessEventEventHandler OnProcessEvent;
		public delegate void OnProcessEventEventHandler(ProcessEvent evt);
		public event OnProcessProgressEventEventHandler OnProcessProgressEvent;
		public delegate void OnProcessProgressEventEventHandler(ProcessProgressEvent evt);
		public event OnProcessCompleteEventHandler OnProcessComplete;
		public delegate void OnProcessCompleteEventHandler(bool closeForm, string exportFilePath, bool exportLogs);
		public event StatusBarEventEventHandler StatusBarEvent;
		public delegate void StatusBarEventEventHandler(string message, string popupText);
		public event ShowReportEventEventHandler ShowReportEvent;
		public delegate void ShowReportEventEventHandler(System.Data.DataTable datasource, bool maxlengthExceeded);
		public event ErrorReportEventEventHandler ErrorReportEvent;
		public delegate void ErrorReportEventEventHandler(System.Collections.IDictionary row);
		public event ShutdownEventEventHandler ShutdownEvent;
		public delegate void ShutdownEventEventHandler();
		public event FieldMappedEventHandler FieldMapped;
		public delegate void FieldMappedEventHandler(string sourceField, string workspaceField);
		public event RecordProcessedEventHandler RecordProcessed;
		public delegate void RecordProcessedEventHandler(long recordNumber);
		public event IncrementRecordCountEventHandler IncrementRecordCount;
		public delegate void IncrementRecordCountEventHandler();
		#endregion

		#region "Event Throwers"

		public void Shutdown()
		{
			if (ShutdownEvent != null) {
				ShutdownEvent();
			}
		}

		public void RaiseRecordProcessed(long recordNumber)
		{
			if (RecordProcessed != null) {
				RecordProcessed(recordNumber);
			}
		}

		public void RaiseFieldMapped(string sourceField, string workspaceField)
		{
			if (FieldMapped != null) {
				FieldMapped(sourceField, workspaceField);
			}
		}

		public void RaiseStatusEvent(string recordInfo, string message)
		{
			ProcessEvent evt = new ProcessEvent(ProcessEventTypeEnum.Status, recordInfo, message);
			if (OnProcessEvent != null) {
				OnProcessEvent(evt);
			}
			WriteToFile(evt);
		}

		public void RaiseWarningEvent(string recordInfo, string message)
		{
			ProcessEvent evt = new ProcessEvent(ProcessEventTypeEnum.Warning, recordInfo, message);
			if (OnProcessEvent != null) {
				OnProcessEvent(evt);
			}
			WriteToFile(evt);
		}

		public void RaiseErrorEvent(string recordInfo, string message)
		{
			ProcessEvent evt = new ProcessEvent(ProcessEventTypeEnum.Error, recordInfo, message);
			if (OnProcessEvent != null) {
				OnProcessEvent(evt);
			}
			WriteToFile(evt);
			WriteError(recordInfo, message);
		}

		public void RaiseProgressEvent(Int64 totalRecords, Int64 totalRecordsProcessed, Int64 totalRecordsProccessedWithWarnings, Int64 totalRecordsProcessedWithErrors, DateTime startTime, DateTime endTime, string totalRecordsDisplay = null, string totalRecordsProcessedDisplay = null, IDictionary args = null)
		{
			if (OnProcessProgressEvent != null) {
				OnProcessProgressEvent(new ProcessProgressEvent(totalRecords, totalRecordsProcessed, totalRecordsProccessedWithWarnings, totalRecordsProcessedWithErrors, startTime, endTime, totalRecordsDisplay, totalRecordsProcessedDisplay, args));
			}
		}

		public void RaiseProcessCompleteEvent(bool closeForm = false, string exportFilePath = "", bool exportLogs = false)
		{
			if (OnProcessComplete != null) {
				OnProcessComplete(closeForm, exportFilePath, exportLogs);
			}
			if ((_errorsWriter != null))
				_errorsWriter.Close();
			if ((_outputWriter != null))
				_outputWriter.Close();
			if (!closeForm && !string.IsNullOrEmpty(_errorsFileName)) {
				object[] o = this.BuildErrorReportDatasource();
				if (ShowReportEvent != null) {
					ShowReportEvent((System.Data.DataTable)o[0], Convert.ToBoolean(o[1]));
				}
			}
		}

		public void RaiseFatalExceptionEvent(Exception ex)
		{
			if (OnProcessFatalException != null) {
				OnProcessFatalException(ex);
			}
			WriteError("FATAL ERROR", ex.ToString());
			ProcessEvent evt = new ProcessEvent(ProcessEventTypeEnum.Error, "", ex.ToString());
			WriteToFile(evt);
		}

		public void RaiseStatusBarEvent(string message, string popupText)
		{
			if (StatusBarEvent != null) {
				StatusBarEvent(message, popupText);
			}
		}

		public void RaiseReportErrorEvent(System.Collections.IDictionary row)
		{
			if (ErrorReportEvent != null) {
				ErrorReportEvent(row);
			}
		}

		public void RaiseCountEvent()
		{
			if (IncrementRecordCount != null) {
				IncrementRecordCount();
			}
		}

		#endregion

		private object[] BuildErrorReportDatasource()
		{
			ErrorFileReader reader = new ErrorFileReader(false);
			return (object[])reader.ReadFile(_errorsFileName);
		}

		#region "File IO"

		public void SaveOutputFile(string fileName)
		{
			System.IO.File.Move(_tempFileName, fileName);
		}

		public void ExportErrorReport(string filename)
		{
			if ((_errorsWriter != null)) {
				try {
					_errorsWriter.Flush();
				} catch {
				}
				try {
					_errorsWriter.Close();
				} catch {
				}
			}
			if (_errorsFileName == null || string.IsNullOrEmpty(_errorsFileName) || !System.IO.File.Exists(_errorsFileName)) {
				System.IO.File.Create(filename);
			} else {
				System.IO.File.Copy(_errorsFileName, filename);
			}
		}

		private void WriteToFile(ProcessEvent evt)
		{
			//TODO:
			//if (!Config.LogAllEvents)
			return;
			//if (!this.InSafeMode) {
			//	System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(evt.GetType());
			//	if (_outputWriter == null || _outputWriter.BaseStream == null) {
			//		this.OpenFile();
			//	}
			//	serializer.Serialize(_outputWriter.BaseStream, evt);
			//}
		}

		private void WriteError(string key, string description)
		{
			if (string.IsNullOrEmpty(_errorsFileName))
				_errorsFileName = System.IO.Path.GetTempFileName();

			if (_errorsWriter == null || _errorsWriter.BaseStream == null) {
				_errorsWriter = new System.IO.StreamWriter(_errorsFileName, true);
			}
			key = key.Replace("\"", "\"\"");
			description = description.Replace("\"", "\"\"");
			_errorsWriter.WriteLine(string.Format("\"{1}{0}Error{0}{2}{0}{3}\"", "\",\"", key, description, System.DateTime.Now.ToString()));
		}

		private void OpenFile()
		{
			_tempFileName = System.IO.Path.GetTempFileName();
			_outputWriter = new System.IO.StreamWriter(_tempFileName, false);
			_outputWriter.WriteLine("<ProcessEvents>");
		}

		private void CloseFile()
		{
			_outputWriter.WriteLine("</ProcessEvents>");
			if ((_outputWriter != null)) {
				_outputWriter.Close();
			}
		}

		#endregion

	}
}
