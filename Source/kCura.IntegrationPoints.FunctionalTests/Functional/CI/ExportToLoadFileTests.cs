using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestType.UI, TestType.MainFlow]
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

        [IdentifiedTest("644f89a0-0642-11ec-9a03-0242ac130003")]
        public void ExportToLoadFile_Natives_GoldFlow()
        {
            _testImplementation.ExportToLoadFilesNativesGoldFlow();
        }

    }
}
