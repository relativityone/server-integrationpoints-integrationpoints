using System.Threading.Tasks;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.ScheduleQueue.Core.Validation
{
    internal class ScheduledJobConsecutiveFailsValidator : IJobPreValidator
    {
        private readonly IConfig _config;
        private readonly IScheduleRuleFactory _scheduleRuleFactory;

        public ScheduledJobConsecutiveFailsValidator(IConfig config, IScheduleRuleFactory scheduleRuleFactory)
        {
            _config = config;
            _scheduleRuleFactory = scheduleRuleFactory;
        }

        public Task<PreValidationResult> ValidateAsync(Job job)
        {
            PreValidationResult result = PreValidationResult.Success;

            IScheduleRule scheduleRule = _scheduleRuleFactory.Deserialize(job);

            if (scheduleRule != null && scheduleRule.GetNumberOfContinuouslyFailedScheduledJobs() > _config.MaxFailedScheduledJobsCount)
            {
                result = PreValidationResult.InvalidJob(
                    $"Scheduled Job has failed {scheduleRule.GetNumberOfContinuouslyFailedScheduledJobs()} times and therefore was stopped by the system. " +
                    $"The schedule won't be restored until Integration Point update.",
                    true);
            }

            return Task.FromResult(result);
        }
    }
}
