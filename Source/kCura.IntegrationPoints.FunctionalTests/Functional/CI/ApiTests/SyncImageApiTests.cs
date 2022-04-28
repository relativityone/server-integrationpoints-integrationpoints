using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI.ApiTests
{
    [TestType.MainFlow]
    public class SyncImageApiTests : SyncApiTestsBase
    {
        public SyncImageApiTests() : base(nameof(SyncImageApiTests))
        {
            Implementation = new SyncImageApiTestsImplementation(this);
        }

    }
}
