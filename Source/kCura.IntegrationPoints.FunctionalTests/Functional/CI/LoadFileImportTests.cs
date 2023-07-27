using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestType.UI]
    [TestType.MainFlow]
    public class LoadFileImportTests : TestsBase
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

        [Test]
        public void LoadNativesFromLoadFileGoldFlow()
        {
            _testImplementation.ImportNativesFromLoadFileGoldFlow();
        }
    }
}
