using System;
using Castle.Core.Internal;
using Relativity.API;

namespace kCura.IntegrationPoints.Domain.EnvironmentalVariables
{
    public sealed class KubernetesMode : IKubernetesMode
    {
        private bool? _isEnabled;
        private readonly IAPILog _logger;

        public KubernetesMode(IAPILog logger)
        {
            _logger = logger;
        }

        public bool IsEnabled()
        {
            if (_isEnabled != null)
            {
                return (bool)_isEnabled;
            }

            string kubernetesModeEnabled = Environment.GetEnvironmentVariable("C4.ADS.AGENT")?.ToLower();
            _isEnabled = kubernetesModeEnabled == "true";
            _logger.LogInformation("Agent is running in Kubernetes Mode - {kubernetesMode}", _isEnabled);

            return (bool)_isEnabled;
        }
    }
}
