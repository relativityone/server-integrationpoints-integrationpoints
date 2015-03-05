using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobTracker
	{
		private readonly IWorkspaceDBContext _context;
		public JobTracker(IWorkspaceDBContext context)
		{
			_context = context;
		}

		public string GenerateTableTempTableName(Job job, string batchID)
		{
			return string.Format("JobTracker_{0}_{1}_{2}", job.WorkspaceID, job.RootJobId);
		}

		public void CreateTrackingEntry(Job job, string batchId)
		{
			var table = GenerateTableTempTableName(job, batchId);
		}

		public bool CheckEntries(Job job)
		{
			return false;
		}

	}
}
