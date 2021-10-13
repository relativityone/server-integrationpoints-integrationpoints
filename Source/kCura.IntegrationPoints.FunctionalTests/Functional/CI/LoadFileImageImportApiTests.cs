using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestType.MainFlow]
    public class LoadFileImageImportApiTests : TestsBase
    {
        private readonly ImportServiceManagerTest _importServiceTest;

        public LoadFileImageImportApiTests() : base(nameof(LoadFileImageImportApiTests))
        {
            _importServiceTest = new ImportServiceManagerTest(this);
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            base.OnSetUpFixture();
            _importServiceTest.OnSetUpFixture();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            base.OnTearDownFixture();
        }

        [IdentifiedTest("b7d92b95-acbf-46fd-a424-749b13167f23")]
        public void TestImportServiceManager()
        {
            _importServiceTest.RunIntegrationPointAndCheckCorectness().ConfigureAwait(false);
        }
    }
}
