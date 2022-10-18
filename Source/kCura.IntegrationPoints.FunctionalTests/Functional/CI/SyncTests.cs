using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Toggles;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Identification;
using Relativity.Toggles;

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
        [TestType.Critical]
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
        public async Task Entities_GoldFlow()
        {
            IToggleProvider toggleProvider = SqlToggleProvider.Create();
            try
            {
                await toggleProvider.SetAsync<EnableSyncNonDocumentFlowToggle>(true).ConfigureAwait(false);
                _testsImplementation.EntitiesPushGoldFlow();
            }
            finally
            {
                await toggleProvider.SetAsync<EnableSyncNonDocumentFlowToggle>(false).ConfigureAwait(false);
            }
        }
    }
}
