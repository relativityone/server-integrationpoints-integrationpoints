using System;
using System.Collections;

namespace kCura.Windows.Process
{
	public class ProcessProgressEvent
	{

		#region "Members"
		private DateTime _startTime;
		private DateTime _endTime;
		private Int64 _totalRecords;
		private Int64 _totalRecordsProcessed;
		private Int64 _totalRecordsProcessedWithWarnings;
		private Int64 _totalRecordsProcessedWithErrors;
		private string _totalRecordsDisplay;
		private string _totalRecordsProcessedDisplay;
			#endregion
		private IDictionary _statusSuffixEntries;

		#region "Accessors"
		public DateTime StartTime {
			get { return _startTime; }
			set { _startTime = value; }
		}

		public DateTime EndTime {
			get { return _endTime; }
			set { _endTime = value; }
		}

		public Int64 TotalRecords {
			get { return _totalRecords; }
			set { _totalRecords = value; }
		}

		public Int64 TotalRecordsProcessed {
			get { return _totalRecordsProcessed; }
			set { _totalRecordsProcessed = value; }
		}

		public Int64 TotalRecordsProcessedWithWarnings {
			get { return _totalRecordsProcessedWithWarnings; }
			set { _totalRecordsProcessedWithWarnings = value; }
		}

		public Int64 TotalRecordsProcessedWithErrors {
			get { return _totalRecordsProcessedWithErrors; }
			set { _totalRecordsProcessedWithErrors = value; }
		}

		public string TotalRecordsDisplay {
			get { return _totalRecordsDisplay; }
			set { _totalRecordsDisplay = value; }
		}

		public string TotalRecordsProcessedDisplay {
			get { return _totalRecordsProcessedDisplay; }
			set { _totalRecordsProcessedDisplay = value; }
		}

		public IDictionary StatusSuffixEntries {
			get { return _statusSuffixEntries; }
		}

		#endregion

		public ProcessProgressEvent(Int64 totRecs, Int64 totRecsProc, Int64 totRecsProcWarn, Int64 totRecsProcErr, DateTime sTime, DateTime eTime, string totRecsDisplay, string totRecsProcDisplay, IDictionary statusSuffixEntries)
		{
			this.TotalRecords = totRecs;
			this.TotalRecordsProcessed = totRecsProc;
			if ((totRecsDisplay != null)) {
				this.TotalRecordsDisplay = totRecsDisplay;
			} else {
				this.TotalRecordsDisplay = totRecs.ToString();
			}
			if ((totRecsProcDisplay != null)) {
				this.TotalRecordsProcessedDisplay = totRecsProcDisplay;
			} else {
				this.TotalRecordsProcessedDisplay = totRecsProc.ToString();
			}
			this.TotalRecordsProcessedWithWarnings = totRecsProcWarn;
			this.TotalRecordsProcessedWithErrors = totRecsProcErr;
			this.StartTime = sTime;
			this.EndTime = eTime;
			_statusSuffixEntries = statusSuffixEntries;
		}

		public ProcessProgressEvent(Int64 totRecs, Int64 totRecsProc, Int64 totRecsProcWarn, Int64 totRecsProcErr, DateTime sTime, DateTime eTime, string totRecsDisplay, string totRecsProcDisplay) : this(totRecs, totRecsProc, totRecsProcWarn, totRecsProcErr, sTime, eTime, totRecsDisplay, totRecsProcDisplay, null)
		{
		}

	}
}
