using System;
using Newtonsoft.Json;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Logs <see cref="Metric"/>s to Splunk. Uses the Relativity <see cref="ISyncLog"/>
	///     system to perform the logging. The Relativity instance on which this is
	///     running should:
	///         1) have logs for this application sent to the Splunk sink;
	///         2) have the log level for this application set to at least Information.
	/// </summary>
	internal sealed class SplunkSyncMetricsSink : ISyncMetricsSink
	{
		private readonly ISyncLog _logger;

		/// <summary>
		///     Creates a new instance of <see cref="SplunkSyncMetricsSink"/>.
		/// </summary>
		/// <param name="logger">Logger to use for logging metrics</param>
		public SplunkSyncMetricsSink(ISyncLog logger)
		{
			_logger = logger;
		}

		/// <inheritdoc />
		public void Log(Metric metric)
		{
			try
			{
				string propertiesJson = JsonConvert.SerializeObject(metric, new JsonSerializerSettings()
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				_logger.LogInformation("Sending metric {MetricName} with properties: {MetricProperties}",
					metric.Name, propertiesJson);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send metric {MetricName}.", metric.Name);
			}
		}
	}
}
