using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestType.Performance]
    public class CiPerformanceTests : TestsBase 
    {
        private readonly PerformanceTestsImplementation _implementation;
        private const int RunCount = 1;

        public CiPerformanceTests() : base(nameof(CiPerformanceTests))
        {
            _implementation = new PerformanceTestsImplementation(this); 
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            base.OnSetUpFixture();
            _implementation.OnSetUpFixture(RunCount);
        }

        [OneTimeTearDown] 
        public void OneTimeTeardown()
        {
            base.OnTearDownFixture();
            _implementation.OnTearDownFixture();
        }

        [IdentifiedTest("5859F168-7A8F-4E58-8200-EBB89132DE87")]
        public async Task SyncPerformanceTest()
        {
            double averageRunTimeInSeconds = await _implementation.RunPerformanceBenchmark(RunCount).ConfigureAwait(false);

            averageRunTimeInSeconds.Should().BeLessOrEqualTo(PerformanceTestsConstants.MAX_AVERAGE_DURATION_S);
        }
    }
}