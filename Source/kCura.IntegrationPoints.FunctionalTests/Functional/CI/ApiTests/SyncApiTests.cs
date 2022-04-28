using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI.ApiTests
{
    [TestType.MainFlow]
    public class SyncApiTests : SyncApiTestsBase
    {
        public SyncApiTests() : base(nameof(SyncApiTests))
        {
            Implementation = new SyncApiTestsImplementation(this);
        }

        protected override SyncApiTestsImplementationBase Implementation { get; }
    }
}