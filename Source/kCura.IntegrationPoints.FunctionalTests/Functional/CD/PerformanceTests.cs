using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CD
{
    [TestExecutionCategory.RAPCD.Verification.NonFunctional]
    [TestType.Performance]
    public class PerformanceTests : TestsBase
    {
        private readonly PerformanceTestsImplementation _implementation;

        public PerformanceTests() : base(nameof(PerformanceTests))
        {
            _implementation = new PerformanceTestsImplementation(this);
        }

        [OneTimeSetUp]
        public void OneTimeSetup() => _implementation.OnSetUpFixture(PerformanceTestsConstants.RUN_COUNT);

        [OneTimeTearDown]
        public void OneTimeTeardown() => _implementation.OnTearDownFixture();

        [IdentifiedTest("601D1C86-18A4-45AD-ABF5-38FE1D3BC99C")]
        public async Task SyncPerformanceTest()
        {
            double averageRunTimeInSeconds = await _implementation.RunPerformanceBenchmark(PerformanceTestsConstants.RUN_COUNT).ConfigureAwait(false);

            averageRunTimeInSeconds.Should().BeLessOrEqualTo(PerformanceTestsConstants.MAX_AVERAGE_DURATION_S);
        }
    }
}
