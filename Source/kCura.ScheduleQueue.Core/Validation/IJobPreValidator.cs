using kCura.IntegrationPoints.Data;
using System.Threading.Tasks;

namespace kCura.ScheduleQueue.Core.Validation
{
    public interface IJobPreValidator
    {
        Task<PreValidationResult> ValidateAsync(Job job);
    }
}
