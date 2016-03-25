using System;

namespace kCura.IntegrationPoints.Services.Interfaces.Private.Models
{
	public class JobHistoryModel
	{
		public int Documents { get; set; }

		public DateTime Date { get; set; }

		public string WorkspaceName { get; set; }
	}
}
