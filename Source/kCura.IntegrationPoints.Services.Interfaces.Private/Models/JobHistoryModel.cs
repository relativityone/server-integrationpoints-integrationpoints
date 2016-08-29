using System;

namespace kCura.IntegrationPoints.Services
{
	public class JobHistoryModel
	{
		public int ItemsTransferred { get; set; }

		public DateTime EndTimeUtc { get; set; }

		public string DestinationWorkspace { get; set; }
	}
}
