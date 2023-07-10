using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
    [TestType.UI]
    [TestType.MainFlow]
    [Ignore("REL-753202")]
    public class SyncTests : TestsBase
    {
        private readonly SyncTestsImplementation _testsImplementation;

        public SyncTests() : base(nameof(SyncTests))
        {
            _testsImplementation = new SyncTestsImplementation(this);
        }

        protected override void OnSetUpFixture()
        {
            base.OnSetUpFixture();
        }

        protected override void OnTearDownFixture()
        {
            base.OnTearDownFixture();

            _testsImplementation.OnTearDownFixture();
        }

        [Test]
        public void SavedSearch_NativesAndMetadata_GoldFlow()
        {
            _testsImplementation.SavedSearchNativesAndMetadataGoldFlow();
        }

        [TestCase(YesNo.No)]
        [TestCase(YesNo.Yes)]
        public void Production_Images_GoldFlow(YesNo copyFilesToRepository)
        {
            _testsImplementation.ProductionImagesGoldFlow(copyFilesToRepository);
        }

        [Test]
        public void Entities_GoldFlow()
        {
            _testsImplementation.EntitiesPushGoldFlow();
        }
    }
}
