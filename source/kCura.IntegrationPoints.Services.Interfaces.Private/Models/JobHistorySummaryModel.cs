using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services.Interfaces.Private.Models
{
	public class JobHistorySummaryModel
	{
		public IEnumerable<JobHistoryModel> Data { get; set; }

		public Int64 TotalAvailable { get; set; }

		public Int64 TotalDocumentsPushed { get; set; }
	}
}
