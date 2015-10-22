﻿using System;

namespace kCura.IntegrationPoints.Data.Logging
{
	public class SystemEventLoggingService
	{
		public static void WriteErrorEvent(string source, string logName, Exception ex)
		{
			if (!System.Diagnostics.EventLog.SourceExists(source))
				System.Diagnostics.EventLog.CreateEventSource(source, logName);

			System.Diagnostics.EventLog.WriteEntry(source,
				Utils.GetPrintableException(ex),
				System.Diagnostics.EventLogEntryType.Error);
		}
	}
}
