using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;
using System.Threading.Tasks;

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

        [IdentifiedTest("b7d92b95-acbf-46fd-a424-749b13167f23")]
        public async Task ImportImageFromLoadFile()
        {
            await _testImplementation.RunIntegrationPointAndCheckCorectness().ConfigureAwait(false);
        }
    }
}
