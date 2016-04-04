using System;

namespace kCura.Windows.Process
{
	[Serializable()]
	public class ProcessEvent
	{
		public DateTime DateTime;
		public ProcessEventTypeEnum Type;
		public string RecordInfo;

		public string Message;

		public ProcessEvent()
		{
		}

		public ProcessEvent(ProcessEventTypeEnum typeValue, string recordInfoValue, string messageValue)
		{
			this.DateTime = System.DateTime.Now;
			this.Type = typeValue;
			this.RecordInfo = recordInfoValue;
			this.Message = messageValue;
		}
	}
}
