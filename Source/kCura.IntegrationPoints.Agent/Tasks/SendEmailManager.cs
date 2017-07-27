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
	    private readonly IAPILog _logger;


        public SendEmailManager(ISerializer serializer, IJobManager jobManager, IHelper helper) : base(helper)
		{
			_serializer = serializer;
			_jobManager = jobManager;
		    _logger = helper.GetLoggerFactory().GetLogger().ForContext<SendEmailManager>();
		}

		public override IEnumerable<string> GetUnbatchedIDs(Job job)
		{		    
            if (!string.IsNullOrEmpty(job?.JobDetails))
			{
			    LogGettingUnbatchedIDs(job);
                return _serializer.Deserialize<EmailMessage>(job.JobDetails).Emails;
			}
			return new List<string>();
		}

		public override void CreateBatchJob(Job job, List<string> batchIDs)
		{
		    LogCreateBatchJobStart(job, batchIDs);
            EmailMessage message = _serializer.Deserialize<EmailMessage>(job.JobDetails);
			message.Emails = batchIDs;
			_jobManager.CreateJob(job, message, TaskType.SendEmailWorker);
		    LogCreateBatchJobEnd(job, batchIDs);
		}

	    private void LogCreateBatchJobEnd(Job job, List<string> batchIDs)
	    {
	        _logger.LogInformation("Finished creating batch job: {Job}, ids: {batchIDs}", job, batchIDs);
	    }

	    private void LogCreateBatchJobStart(Job job, List<string> batchIDs)
	    {
	        _logger.LogInformation("Started creating batch job: {JobId}, ids: {batchIDs}", job.JobId, batchIDs);
	    }

	    private void LogGettingUnbatchedIDs(Job job)
	    {
	        _logger.LogInformation("Getting unbatched IDs in Send Email Manager for job {JobId}.", job.JobId);
	    }
    }
}