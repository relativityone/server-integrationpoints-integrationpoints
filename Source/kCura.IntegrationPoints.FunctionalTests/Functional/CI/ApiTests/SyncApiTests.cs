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
            _implementation.OneTimeSetup();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _implementation.OneTimeTeardown();
        }

        [Test]
        [Ignore("REL-862989")]
        public async Task JobRetryTest()
        {
            await _implementation.RunAndRetryIntegrationPoint().ConfigureAwait(false);
        }

        [Test]
        [TestType.Critical]
        public async Task JobRunTest()
        {
            await _implementation.RunIntegrationPoint().ConfigureAwait(false);
        }
    }
}
