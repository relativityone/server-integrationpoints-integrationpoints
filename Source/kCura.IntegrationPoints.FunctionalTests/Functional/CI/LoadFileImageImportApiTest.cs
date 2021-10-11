using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestType.MainFlow]
    public class LoadFileImageImportApiTest : TestsBase
    {
        private readonly ImportServiceManagerTest _importServiceTest;

        public LoadFileImageImportApiTest() : base(nameof(LoadFileImageImportApiTest))
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
        public async Task TestImportServiceManager()
        {
            int docs = await _importServiceTest.RunIntegrationPointAndCheckCorectness().ConfigureAwait(false);
            Assert.AreEqual(10, docs);
        }
    }
}
