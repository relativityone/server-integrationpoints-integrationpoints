using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI.ApiTests
{
    [TestType.MainFlow]
    public class LoadFileImageImportApiTests : TestsBase
    {
        private readonly LoadFileImageImportApiTestImplementation _testImplementation;

        public LoadFileImageImportApiTests() : base(nameof(LoadFileImageImportApiTests))
        {
            _testImplementation = new LoadFileImageImportApiTestImplementation(this);
        }

        protected override void OnSetUpFixture()
        {
            base.OnSetUpFixture();
            _testImplementation.OnSetUpFixture();
        }

        [Test]
        [TestType.Critical]
        public async Task ImportImageFromLoadFile()
        {
            await _testImplementation.RunIntegrationPointAndCheckCorectness().ConfigureAwait(false);
        }
    }
}
