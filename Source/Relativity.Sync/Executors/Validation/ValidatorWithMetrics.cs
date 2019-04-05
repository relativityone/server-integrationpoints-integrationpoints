using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class ValidatorWithMetrics : IValidator
	{
		private readonly IValidator _validator;
		private readonly ISyncMetrics _metrics;

		public ValidatorWithMetrics(IValidator validator, ISyncMetrics metrics)
		{
			_validator = validator;
			_metrics = metrics;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			ValidationResult result = await _validator.ValidateAsync(configuration, token).ConfigureAwait(false);
			if (!result.IsValid)
			{
				_metrics.CountOperation(_validator.GetType().Name, ExecutionStatus.Failed);
			}

			return result;
		}
	}
}