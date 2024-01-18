using System.Collections.Generic;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Telemetry
{
    /// <summary>
    ///     Logs APM metrics to Relativity APM.
    /// </summary>
    internal sealed class APMClient : IAPMClient
    {
        private readonly IAPM _apm;

        /// <summary>
        ///     Creates an instance of <see cref="APMClient"/>.
        /// </summary>
        /// <param name="apm">Instance of <see cref="IAPM"/> instance to which this client forwards metrics</param>
        public APMClient(IAPM apm)
        {
            _apm = apm;
        }

        /// <inheritdoc />
        public void Count(string name, Dictionary<string, object> customData)
        {
            _apm.CountOperation(name, customData: customData).Write();
        }

        /// <inheritdoc/>
        public void Gauge(string name, string correlationId, Dictionary<string, object> customData)
        {
            var jobDetails = _apm.GaugeOperation(name, operation: () => 1, correlationID: correlationId, customData: customData);
            jobDetails.Write();
        }
    }
}
