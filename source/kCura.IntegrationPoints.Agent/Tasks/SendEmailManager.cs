using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.BatchProcess;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SendEmailManager : BatchManagerBase<Core.Models.EmailMessage>
	{
		private readonly ISerializer _serializer;
		private readonly IJobManager _jobManager;
		public SendEmailManager(ISerializer serializer, IJobManager jobManager)
		{
			_serializer = serializer;
			_jobManager = jobManager;
		}

		public override IEnumerable<EmailMessage> GetUnbatchedIDs(Job job)
		{
			return _serializer.Deserialize<List<Core.Models.EmailMessage>>(job.JobDetails);
		}

		public override void CreateBatchJob(Job job, List<EmailMessage> batchIDs)
		{
			_jobManager.CreateJob(job, batchIDs, TaskType.SendEmailWorker);
		}
	}
}
