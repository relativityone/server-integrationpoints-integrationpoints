using kCura.IntegrationPoints.Domain.EnvironmentalVariables;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class FakeKubernetesMode : IKubernetesMode
    {
        private bool _isEnabled  = false;

        public bool IsEnabled()
        {
            return _isEnabled;
        }

        public void SetIsEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
        }
    }
}
