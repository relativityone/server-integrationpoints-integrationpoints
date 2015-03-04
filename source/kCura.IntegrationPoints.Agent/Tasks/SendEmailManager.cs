using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.BatchProcess;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SendEmailManager : BatchManagerBase<Core.Models.EmailMessage>
	{
		public override IEnumerable<EmailMessage> GetUnbatchedIDs(Job job)
		{
			throw new NotImplementedException();
		}

		public override void CreateBatchJob(Job job, List<EmailMessage> batchIDs)
		{
			throw new NotImplementedException();
		}
	}
}
