using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;

namespace Relativity.IntegrationPoints.Tests.Functional.CI.ApiTests
{
    public class SyncApiTestsBase : TestsBase
    {
        private readonly SyncApiTestsImplementationBase _implementation;

        protected SyncApiTestsBase(string workspaceName, SyncApiTestsImplementationBase implementation) : base(workspaceName)
        {
            _implementation = implementation;
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
