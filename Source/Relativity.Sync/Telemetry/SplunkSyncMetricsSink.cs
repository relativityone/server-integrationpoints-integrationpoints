using System.Collections.Generic;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Logs <see cref="Metric"/>s to Splunk. Uses the Relativity <see cref="ISyncLog"/>
	///     system to perform the logging. The Relativity instance on which this is
	///     running should:
	///         1) have logs for this application sent to the Splunk sink;
	///         2) have the log level for this application set to at least Information.
	/// </summary>
	internal class SplunkSyncMetricsSink : ISyncMetricsSink
	{
		private readonly ISyncLog _logger;
		private readonly IEnvironmentPropertyProvider _envProperties;

		/// <summary>
		///     Creates a new instance of <see cref="SplunkSyncMetricsSink"/>.
		/// </summary>
		/// <param name="logger">Logger to use for logging metrics</param>
		/// <param name="envProperties">Provider of environment properties used to enrich metadata</param>
		public SplunkSyncMetricsSink(ISyncLog logger, IEnvironmentPropertyProvider envProperties)
		{
			_logger = logger;
			_envProperties = envProperties;
		}

		/// <inheritdoc />
		public void Log(Metric metric)
		{
			Dictionary<string, object> parameters = metric.ToDictionary();
			parameters.Add(nameof(_envProperties.InstanceName), _envProperties.InstanceName);
			parameters.Add(nameof(_envProperties.CallingAssembly), _envProperties.CallingAssembly);

			// The message template is not used here, so we'll just log an empty string.
			_logger.LogInformation(string.Empty, parameters);
		}
	}
}
