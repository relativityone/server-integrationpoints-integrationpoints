using System;

namespace kCura.IntegrationPoints.Domain.EnvironmentalVariables
{
    public sealed class KubernetesMode : IKubernetesMode
    {
        public bool IsEnabled
        {
            get
            {
                string kubernetesModeEnabled = Environment.GetEnvironmentVariable("C4.ADS.AGENT")?.ToLower();
                if (kubernetesModeEnabled != null)
                {
                    return kubernetesModeEnabled == "true";
                }

                return false;
            }
            set => throw new InvalidOperationException("Kubernetes mode can't be set. Use dedicated mechanism to change it");
        }
    }
}
