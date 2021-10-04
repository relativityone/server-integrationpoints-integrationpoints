using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestType.UI, TestType.MainFlow]
    class LoadFileImportTests: TestsBase
    {
        private readonly ImportLoadFileTestImplementation _testImplementation;
        private readonly ImportServiceManagerTest _importServiceTest;

        public LoadFileImportTests()
            : base(nameof(LoadFileImportTests))
        {
            _testImplementation = new ImportLoadFileTestImplementation(this);
            _importServiceTest = new ImportServiceManagerTest(this);
        }

        protected override void OnSetUpFixture()
        {
            base.OnSetUpFixture();
            _testImplementation.OnSetUpFixture();
            _importServiceTest.OnSetUpFixture();
        }

        [TestType.Critical]
        [IdentifiedTest("88bf9b08-99c4-4c30-8854-fff22c4dc213")]
        public void LoadNativesFromLoadFileGoldFlow()
        {
            _testImplementation.ImportNativesFromLoadFileGoldFlow();
        }

        [IdentifiedTest("b7d92b95-acbf-46fd-a424-749b13167f23")]
        public void TestImportServiceManager()
        {
            _importServiceTest.RunTest();
        }
    }
}
