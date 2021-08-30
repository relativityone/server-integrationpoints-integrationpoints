using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestType.UI, TestType.MainFlow]
    class LoadFileImportTests: TestsBase
    {
        private readonly ImportLoadFileTestImplementation _testImplementation;

        public LoadFileImportTests()
            : base(nameof(LoadFileImportTests))
        {
            _testImplementation = new ImportLoadFileTestImplementation(this);
        }

        protected override async void OnSetUpFixture()
        {
            base.OnSetUpFixture();
            await _testImplementation.OnSetUpFixture();
        }

        [IdentifiedTest("88bf9b08-99c4-4c30-8854-fff22c4dc213")]
        public void LoadNativesFromLoadFileGoldFlow()
        {
            _testImplementation.ImportNativesFromLoadFileGoldFlow();
        }
    }
}
