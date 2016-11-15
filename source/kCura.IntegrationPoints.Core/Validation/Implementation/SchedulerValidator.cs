using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class SchedulerValidator : IProviderValidator
	{
		private readonly Scheduler _scheduler;

		public SchedulerValidator(Scheduler scheduler)
		{
			_scheduler = scheduler;
		}

		public ValidationResult Validate()
		{
			throw new System.NotImplementedException();
		}
	}
}
