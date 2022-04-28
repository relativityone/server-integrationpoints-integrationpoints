using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;

namespace Relativity.IntegrationPoints.Tests.Functional.CI.ApiTests
{
    public abstract class SyncApiTestsBase : TestsBase
    {
        protected abstract SyncApiTestsImplementationBase Implementation { get;}

        protected SyncApiTestsBase(string workspaceName) : base(workspaceName)
        {
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Implementation.OneTimeSetup();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            Implementation.OneTimeTeardown();
        }

        [Test]
        public async Task JobRetryTest()
        {
            await Implementation.RunAndRetryIntegrationPoint().ConfigureAwait(false);
        }

        [Test]
        public async Task JobRunTest()
        {
            await Implementation.RunIntegrationPoint().ConfigureAwait(false);
        }
    }
}
