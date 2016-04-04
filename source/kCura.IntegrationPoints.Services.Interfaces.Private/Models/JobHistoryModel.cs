using System;

namespace kCura.IntegrationPoints.Services.Interfaces.Private.Models
{
	public class JobHistoryModel
	{
		public int ItemsImported { get; set; }

		public DateTime EndTimeUtc { get; set; }

		public string DestinationWorkspace { get; set; }
	}
}
