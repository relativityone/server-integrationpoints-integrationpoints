using System.Threading.Tasks;

namespace kCura.ScheduleQueue.Core.Validation
{
	public interface IQueueJobValidator
	{
		Task<ValidationResult> ValidateAsync(Job job);
	}
}
