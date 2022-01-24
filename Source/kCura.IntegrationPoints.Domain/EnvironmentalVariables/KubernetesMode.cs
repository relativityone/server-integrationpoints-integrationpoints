using System;
using Castle.Core.Internal;

namespace kCura.IntegrationPoints.Domain.EnvironmentalVariables
{
    public sealed class KubernetesMode : IKubernetesMode
    {
        private bool? _isEnabled;

        public bool IsEnabled
        {
            get
            {
                string kubernetesModeEnabled = "";
                if (_isEnabled == null)
                {
                    kubernetesModeEnabled = Environment.GetEnvironmentVariable("C4.ADS.AGENT")?.ToLower();
                }

                if (!kubernetesModeEnabled.IsNullOrEmpty())
                {
                    _isEnabled = kubernetesModeEnabled == "true";
                }

                return _isEnabled != null && (bool)_isEnabled;
            }
        }
    }
}
