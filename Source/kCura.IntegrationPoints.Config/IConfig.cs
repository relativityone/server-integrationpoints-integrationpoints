using System;

namespace kCura.IntegrationPoints.Config
{
    public interface IConfig
    {
        /// <summary>
        /// The web api path to be used by the import api.
        /// </summary>
        string WebApiPath { get; }

        /// <summary>
        /// Disables the validation of the native file locations.
        /// </summary>
        bool DisableNativeLocationValidation { get; }

        /// <summary>
        /// Disables the validation of native file types.
        /// </summary>
        bool DisableNativeValidation { get; }

        /// <summary>
        /// The batch size for all providers except the Relativity provider.
        /// </summary>
        int BatchSize { get; }

        /// <summary>
        /// The maximum number of failed scheduled jobs for single integration point.
        /// </summary>
        int MaxFailedScheduledJobsCount { get; }

        /// <summary>
        /// Throttling value for metrics
        /// </summary>
        TimeSpan MetricsThrottling { get; }

        /// <summary>
        /// Gets a value indicating whether to send live APM metrics
        /// </summary>
        /// <value>
        ///   <c>true</c> if live APM metrics should be sent; otherwise, <c>false</c>.
        /// </value>
        bool SendLiveApmMetrics { get; }

        /// <summary>
        /// Gets a value indicating whether to send summary APM metrics
        /// </summary>
        /// <value>
        ///   <c>true</c> if summary APM metrics should be sent; otherwise, <c>false</c>.
        /// </value>
        bool SendSummaryMetrics { get; }

        /// <summary>
        /// Gets a value indicating whether to send SUM metrics
        /// </summary>
        /// <value>
        ///   <c>true</c> if SUM metrics should be sent; otherwise, <c>false</c>.
        /// </value>
        bool SendSumMetrics { get; }

        /// <summary>
        /// Gets a value indication wheter external calls duration should be measured
        /// </summary>
        bool MeasureDurationOfExternalCalls { get; }

        /// <summary>
        /// Gets size of mass update batch
        /// </summary>
        int MassUpdateBatchSize { get; }

        /// <summary>
        /// Gets time threshold for long running jobs
        /// </summary>
        TimeSpan RunningJobTimeThreshold { get; }

        /// <summary>
        /// Gets Timeout for Transient State RIP Jobs
        /// </summary>
        TimeSpan TransientStateJobTimeout { get; }

        /// <summary>
        /// Gets max Agent lifetime
        /// </summary>
        TimeSpan AgentMaximumLifetime { get; }
    }
}
