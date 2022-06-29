using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Validation
{
    public class QueueJobValidator : IQueueJobValidator
	{
		private readonly IHelper _helper;
        private readonly IAPILog _log;
        private readonly IJobPreValidator[] _validators;

		public QueueJobValidator(IHelper helper, IAPILog log)
		{
			_helper = helper;
            _log = log.ForContext<QueueJobValidator>();
            _validators = new IJobPreValidator[]
			{
				new WorkspaceExistsValidator(_helper),
				new IntegrationPointExistsValidator(_helper),
				new UserExistsValidator(_helper),
			};
		}

		public async Task<PreValidationResult> ValidateAsync(Job job)
		{
			_log.LogInformation("PreValidation for job {jobId} started...", job.JobId);

			foreach(var validator in _validators)
            {
				_log.LogInformation("Validating {validator}...", validator.GetType());
				PreValidationResult result = await validator.ValidateAsync(job);
				if(!result.IsValid)
                {
					_log.LogError(result.Exception, "Job {jobId} PreValidation failed.", job.JobId);
					return result;
                }
            }

			_log.LogInformation("Job {jobId} PreValidation passed.", job.JobId);

			return PreValidationResult.Success;
		}
	}
}
