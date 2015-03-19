﻿using System;
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
	public class SendEmailManager : BatchManagerBase<string>
	{
		private readonly ISerializer _serializer;
		private readonly IJobManager _jobManager;
		public SendEmailManager(ISerializer serializer, IJobManager jobManager)
		{
			_serializer = serializer;
			_jobManager = jobManager;
		}

		public override IEnumerable<string> GetUnbatchedIDs(Job job)
		{
			if (!string.IsNullOrEmpty(job.JobDetails))
			{
				return _serializer.Deserialize<Core.Models.EmailMessage>(job.JobDetails).Emails;
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
