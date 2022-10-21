using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.CI;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;

namespace Relativity.IntegrationPoints.Tests.Functional.REG_CI
{
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
            _implementation.OneTimeSetup();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _implementation.OneTimeTeardown();
        }

        [Test]
        public async Task JobRunTest()
        {
            await _implementation.RunIntegrationPoint().ConfigureAwait(false);
        }
    }
}
