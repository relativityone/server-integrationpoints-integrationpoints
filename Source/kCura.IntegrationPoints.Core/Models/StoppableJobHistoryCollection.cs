using kCura.IntegrationPoints.Data;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Models
{
	public class StoppableJobHistoryCollection
	{
		public JobHistory[] PendingJobHistory { get; set; } 
		public JobHistory[] ProcessingJobHistory { get; set; }

		public bool HasStoppableJobHistory 
			=> PendingJobHistory.Any() || ProcessingJobHistory.Any();

		public bool HasOnlyPendingJobHistory
			=> PendingJobHistory.Any() && !ProcessingJobHistory.Any();
	}
}