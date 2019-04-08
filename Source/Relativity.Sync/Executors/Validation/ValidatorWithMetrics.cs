using System;
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
		private readonly IStopwatch _stopwatch;

		public ValidatorWithMetrics(IValidator validator, ISyncMetrics metrics, IStopwatch stopwatch)
		{
			_validator = validator;
			_metrics = metrics;
			_stopwatch = stopwatch;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			string metricName = _validator.GetType().Name;
			ValidationResult result = new ValidationResult();
			_stopwatch.Start();

			try
			{
				result = await _validator.ValidateAsync(configuration, token).ConfigureAwait(false);
			}
			catch
			{
				result.IsValid = false;
			}
			finally
			{
				_stopwatch.Stop();
				ExecutionStatus status = result.IsValid ? ExecutionStatus.Completed : ExecutionStatus.Failed;

				_metrics.TimedOperation(metricName, _stopwatch.Elapsed, status);
				if (!result.IsValid)
				{
					_metrics.CountOperation(metricName, status);
				}
			}
			
			return result;
		}
	}
}