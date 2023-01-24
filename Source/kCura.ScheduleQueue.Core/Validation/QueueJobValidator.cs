using System.Threading.Tasks;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Validation
{
    public class QueueJobValidator : IQueueJobValidator
    {
        private readonly IAPILog _log;
        private readonly IJobPreValidator[] _validators;


        public QueueJobValidator(IRelativityObjectManagerFactory relativityObjectManagerFactory, IConfig config, IScheduleRuleFactory scheduleRuleFactory, IAPILog log)
        {
            _log = log.ForContext<QueueJobValidator>();
            _validators = new IJobPreValidator[]
            {
                new WorkspaceExistsValidator(relativityObjectManagerFactory),
                new IntegrationPointExistsValidator(relativityObjectManagerFactory),
                new UserExistsValidator(relativityObjectManagerFactory),
                new ScheduledJobConsecutiveFailsValidator(config, scheduleRuleFactory)
            };
        }

        public async Task<PreValidationResult> ValidateAsync(Job job)
        {
            _log.LogInformation("PreValidation for job {jobId} started...", job.JobId);

            foreach (var validator in _validators)
            {
                _log.LogInformation("Validating {validator}...", validator.GetType());
                PreValidationResult result = await validator.ValidateAsync(job).ConfigureAwait(false);

                if (!result.IsValid)
                {
                    _log.LogError(result.Exception, "Job {jobId} PreValidation failed - {@validationResult}.", job.JobId, result);
                    return result;
                }
            }

            _log.LogInformation("Job {jobId} PreValidation passed.", job.JobId);

            return PreValidationResult.Success;
        }
    }
}
