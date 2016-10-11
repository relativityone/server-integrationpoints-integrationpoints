using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.BatchProcess;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SendEmailManager : BatchManagerBase<string>
	{
		private readonly IJobManager _jobManager;
		private readonly ISerializer _serializer;

		public SendEmailManager(ISerializer serializer, IJobManager jobManager, IHelper helper) : base(helper)
		{
			_serializer = serializer;
			_jobManager = jobManager;
		}

		public override IEnumerable<string> GetUnbatchedIDs(Job job)
		{
			if (!string.IsNullOrEmpty(job?.JobDetails))
			{
				return _serializer.Deserialize<EmailMessage>(job.JobDetails).Emails;
			}
			return new List<string>();
		}

		public override void CreateBatchJob(Job job, List<string> batchIDs)
		{
			EmailMessage message = _serializer.Deserialize<EmailMessage>(job.JobDetails);
			message.Emails = batchIDs;
			_jobManager.CreateJob(job, message, TaskType.SendEmailWorker);
		}
	}
}