using System;

namespace kCura.IntegrationPoints.Services
{
	public class JobHistoryModel
	{
		public int ItemsImported { get; set; }

		public DateTime EndTimeUtc { get; set; }

		public string DestinationWorkspace { get; set; }
	}
}
