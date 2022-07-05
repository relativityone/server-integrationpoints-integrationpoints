using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;

namespace kCura.ScheduleQueue.Core.Validation
{
	public interface IQueueJobValidator
	{
		Task<PreValidationResult> ValidateAsync(Job job);
	}
}
