using kCura.IntegrationPoints.Domain.EnvironmentalVariables;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class FakeKubernetesMode : IKubernetesMode
    {
        public bool IsEnabled { get; set; } = false;
    }
}
