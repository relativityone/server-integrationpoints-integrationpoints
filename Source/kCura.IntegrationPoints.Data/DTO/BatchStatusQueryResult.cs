using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.DTO
{
	public class BatchStatusQueryResult
	{
		public int ProcessingCount { get; set; }
		public int PendingCount { get; set; }
		public int SuspendedCount { get; set; }

		public int BatchTotal => ProcessingCount + PendingCount + SuspendedCount;
	}
}
