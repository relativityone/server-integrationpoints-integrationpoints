using System.Linq;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using kCura.IntegrationPoints.Data.Repositories;
using LanguageExt;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    internal class StatisticsManagerTests : TestsBase
    {
        private IStatisticsManager _sut;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(() => Helper.GetServicesManager()).IsDefault().Named("ObjectManagerStub"));
            _sut = Container.Resolve<IStatisticsManager>();
        }

        [IdentifiedTest("C9A3B83C-E53B-4D25-B4A5-5B4FF7B73BE2")]
        public async Task GetDocumentsTotalForSavedSearchAsync_ShouldPass()
        {
            // Arrange
            int savedSearchArtifactId = SourceWorkspace.SavedSearches.First().ArtifactId;

            // Act 
            long totalDocuments = await _sut
                .GetDocumentsTotalForSavedSearchAsync(SourceWorkspace.ArtifactId, savedSearchArtifactId)
                .ConfigureAwait(false);

            // Assert

        }
    }
}
