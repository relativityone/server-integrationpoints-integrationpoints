using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestType.UI, TestType.MainFlow]
    public class SyncSummaryPageTests : TestsBase
    {
        private readonly SyncSummaryPageTestsImplementation _testsImplementation;

        public SyncSummaryPageTests() : base(nameof(SyncSummaryPageTests))
        {
            _testsImplementation = new SyncSummaryPageTestsImplementation(this);
        }

        protected override void OnSetUpFixture()
        {
            base.OnSetUpFixture();

            _testsImplementation.OnSetUpFixture();
        }

        protected override void OnTearDownFixture()
        {
            base.OnTearDownFixture();

            _testsImplementation.OnTearDownFixture();
        }

        [Test]
        public void SavedSearch_NativesAndMetadata_SummaryPageTest()
        {
            _testsImplementation.SavedSearchNativesAndMetadataSummaryPage();
        }

        [Test]
        public void SavedSearch_Images_SummaryPageTest()
        {
            _testsImplementation.SavedSearchImagesSummaryPage();
        }

        [Test]
        public void Production_Images_SummaryPageTest()
        {
            _testsImplementation.ProductionImagesSummaryPage();
        }

        [Test]
        public void Entities_SummaryPageTest()
        {
            _testsImplementation.EntitiesPushSummaryPage();
        }
    }
}
