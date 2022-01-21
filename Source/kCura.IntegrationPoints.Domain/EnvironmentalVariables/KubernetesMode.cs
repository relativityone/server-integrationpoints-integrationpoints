using System;

namespace kCura.IntegrationPoints.Domain.EnvironmentalVariables
{
    public sealed class KubernetesMode : IKubernetesMode
    {
        public bool Value
        {
            get => Environment.GetEnvironmentVariable("C4.ADS.AGENT").ToLower() == "true";
            set => throw new InvalidOperationException("Kubernetes mode can't be set. Use dedicated mechanism to change it");
        }
    }
}
