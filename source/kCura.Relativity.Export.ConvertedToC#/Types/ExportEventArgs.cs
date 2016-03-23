using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Types
{

	public class ExportEventArgs
	{
		protected Int32 _documentsExported;
		protected Int32 _totalDocuments;
		protected kCura.Windows.Process.EventType _eventType;
		protected string _message;

		protected object _additionalInfo;
		#region "Public Properties"
		public Int32 DocumentsExported {
			get { return _documentsExported; }
			set { _documentsExported = value; }
		}

		public Int32 TotalDocuments {
			get { return _totalDocuments; }
			set { _totalDocuments = value; }
		}

		public kCura.Windows.Process.EventType EventType {
			get { return _eventType; }
			set { _eventType = value; }
		}

		public string Message {
			get { return _message; }
			set { _message = value; }
		}

		public object AdditionalInfo {
			get { return _additionalInfo; }
		}
		#endregion

		public ExportEventArgs(Int32 documentsExported, Int32 totalDocuments, object additionalInfo)
		{
			_documentsExported = documentsExported;
			_totalDocuments = totalDocuments;
			_additionalInfo = additionalInfo;
		}

		public ExportEventArgs(Int32 documentsExported, Int32 totalDocuments, string message, kCura.Windows.Process.EventType eventType, object additionalInfo)
		{
			_documentsExported = documentsExported;
			_totalDocuments = totalDocuments;
			_message = message;
			_eventType = eventType;
			_additionalInfo = additionalInfo;
		}
	}
}
