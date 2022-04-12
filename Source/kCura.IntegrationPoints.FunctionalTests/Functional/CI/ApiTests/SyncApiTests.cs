using NUnit.Framework;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI.ApiTests
{
    [TestType.MainFlow]
    public class SyncApiTests : TestsBase
    {
        private readonly SyncApiTestsImplementation _implementation;

        public SyncApiTests() : base(nameof(SyncApiTests))
        {
            _implementation = new SyncApiTestsImplementation(this);
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            base.OnSetUpFixture();
            _implementation.OnSetUpFixture();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            base.OnTearDownFixture();
        }
       
        [Test]
        public async Task JobRetryTest()
        {
            await _implementation.RunAndRetryIntegrationPoint().ConfigureAwait(false);
        }

        [Test]
        public async Task JobRunTest()
        {
            await _implementation.RunIntegrationPoint().ConfigureAwait(false);
        }
    }
}
