using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestType.UI]
    [TestType.MainFlow]
    public class ExportToLoadFileTests : TestsBase
    {
        private ExportToLoadFileTestImplementation _testImplementation;

        public ExportToLoadFileTests()
            : base(nameof(ExportToLoadFileTests))
        {
            _testImplementation = new ExportToLoadFileTestImplementation(this);
        }

        protected override void OnSetUpFixture()
        {
            base.OnSetUpFixture();
            _testImplementation.OnSetUpFixture();
        }

        [Test]
        [Ignore("REL-753202")]
        public void ExportToLoadFile_Natives_GoldFlow()
        {
            _testImplementation.ExportToLoadFilesNativesGoldFlow();
        }
    }
}
