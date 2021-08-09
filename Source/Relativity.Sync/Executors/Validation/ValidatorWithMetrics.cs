using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Utils;

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
			finally
			{
				_stopwatch.Stop();
				ExecutionStatus status = result.IsValid ? ExecutionStatus.Completed : ExecutionStatus.Failed;

				ValidationMetric metric = new ValidationMetric(metricName)
				{
					ExecutionStatus = status,
					Duration = _stopwatch.Elapsed.TotalMilliseconds,
					FailedCounter = !result.IsValid ? (Counter?)Counter.Increment : null
				};

				_metrics.Send(metric);
			}
			
			return result;
		}

		public bool ShouldValidate(ISyncPipeline pipeline)
		{
			return _validator.ShouldValidate(pipeline);
		}
	}
}