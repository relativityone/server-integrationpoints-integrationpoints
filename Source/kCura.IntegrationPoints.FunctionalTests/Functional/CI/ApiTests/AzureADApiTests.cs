using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;
using Relativity.Toggles;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Functional.CI.ApiTests
{
    public class AzureADApiTests : TestsBase
    {
        private const string _ADS_IN_K8S_TOGGLE = "Relativity.ADS.Agents.Toggles.ApplicationInstallationWorkerIsActive";

        private readonly AzureADTestImplementation _testImplementation;

        public AzureADApiTests() : base(nameof(AzureADApiTests))      
        {
            _testImplementation = new AzureADTestImplementation(this);
        }

        protected override void OnSetUpFixture()
        {
            base.OnSetUpFixture();
            
            _testImplementation.OnSetupFixture();
        }

        [Test]
        public async Task ImportEntityWithAzureADProvider()
        {
            IToggleProviderExtended toggleProvider = SqlToggleProvider.Create();
            try
            {
                await toggleProvider.SetAsync(_ADS_IN_K8S_TOGGLE, true).ConfigureAwait(false);
                await _testImplementation.ImportEntityWithAzureADProvider().ConfigureAwait(false);
            }
            finally
            {
                await toggleProvider.SetAsync(_ADS_IN_K8S_TOGGLE, false).ConfigureAwait(false);
            }
        }
    }
}
