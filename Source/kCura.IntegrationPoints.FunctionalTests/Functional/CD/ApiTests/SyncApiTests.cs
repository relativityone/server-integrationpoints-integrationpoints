using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CD.ApiTests
{
    [TestType.MainFlow]
    public class SyncApiTests : TestsBase
    {
        private readonly SyncApiTestsImplementation _implementation;

        public SyncApiTests() : base(nameof(SyncTests))
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

        [IdentifiedTest("ACB959DF-6C5A-42A3-AFBC-3E180B500B72")]
        [TestExecutionCategory.RAPCD.Verification.Functional]
        public async Task JobRunTest()
        {
            await _implementation.RunIntegrationPoint().ConfigureAwait(false);
        }
    }
}
