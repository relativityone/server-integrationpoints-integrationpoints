using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;
using WorkloadDiscovery;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    public class WorkLoadSizeTests : TestsBase
    {
        private readonly WorkloadSizeTestImplementation _workloadSizeTestImplementation;

        public WorkLoadSizeTests() : base(nameof(WorkLoadSizeTests))
        {
            _workloadSizeTestImplementation = new WorkloadSizeTestImplementation();
        }

        [Test]
        public async Task ShouldGetWorkloadSize()
        {
            WorkloadSize expectedValue = WorkloadSize.One;
            // ARRANGE
            _workloadSizeTestImplementation.AddMockJobToSqlTable();

            // ACT
            WorkloadSize workloadSizeReturned = await _workloadSizeTestImplementation.RequestWorkloadSizeFromRIPAsync().ConfigureAwait(false);

            // ASSERT
            workloadSizeReturned.Should().Be(expectedValue);
        }

        protected override void OnTearDownFixture()
        {
            _workloadSizeTestImplementation.RemoveMockJobFromSqlTable();
            base.OnTearDownFixture();
        }
    }
}
