using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Functional.CI.ApiTests
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
            await _testImplementation.ImportEntityWithAzureADProvider().ConfigureAwait(false);
        }
    }
}
