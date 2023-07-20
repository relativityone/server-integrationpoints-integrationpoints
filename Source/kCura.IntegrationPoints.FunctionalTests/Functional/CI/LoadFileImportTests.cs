using NUnit.Framework;

using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestType.UI, TestType.MainFlow]
    public class LoadFileImportTests: TestsBase
    {
        private readonly ImportLoadFileTestImplementation _testImplementation;

        public LoadFileImportTests()
            : base(nameof(LoadFileImportTests))
        {
            _testImplementation = new ImportLoadFileTestImplementation(this);
        }

        protected override void OnSetUpFixture()
        {
            base.OnSetUpFixture();
            _testImplementation.OnSetUpFixture();
        }

        [TestType.Critical]
        [IdentifiedTest("88bf9b08-99c4-4c30-8854-fff22c4dc213")]
        [Ignore("REL-841500: Resolve RIP functional test failures.")]
        public void LoadNativesFromLoadFileGoldFlow()
        {
            _testImplementation.ImportNativesFromLoadFileGoldFlow();
        }
    }
}
