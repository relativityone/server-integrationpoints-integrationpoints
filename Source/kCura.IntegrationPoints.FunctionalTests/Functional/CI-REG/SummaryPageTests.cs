using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.CI;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;

namespace Relativity.IntegrationPoints.Tests.Functional.CI_REG
{
    internal class SummaryPageTests : TestsBase
    {
        private readonly SyncSummaryPageTestsImplementation _testsImplementation;

        public SummaryPageTests() : base(nameof(SummaryPageTests))
        {
            _testsImplementation = new SyncSummaryPageTestsImplementation(this);
        }

        [Test]
        [Ignore("NotWorking")]
        public void SavedSearch_NativesAndMetadata_SummaryPageTest()
        {
            _testsImplementation.SavedSearchNativesAndMetadataSummaryPage();
        }

        protected override void OnTearDownFixture()
        {
            base.OnTearDownFixture();
            _testsImplementation.OnTearDownFixture();
        }
    }
}
