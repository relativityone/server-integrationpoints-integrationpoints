using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.CI;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;

namespace Relativity.IntegrationPoints.Tests.Functional.REG
{
    public class AzureADApiTests : TestsBase
    {
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
            await _testImplementation.ImportEntityWithAzureADProviderAsync().ConfigureAwait(false);
        }
    }
}
