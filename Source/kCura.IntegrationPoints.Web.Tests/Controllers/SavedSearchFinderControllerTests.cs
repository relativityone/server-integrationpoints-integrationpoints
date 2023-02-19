using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
    [TestFixture, Category("Unit")]
    public class SavedSearchFinderControllerTests
    {
        private const int _WKSP_ID = 1;
        private SavedSearchFinderController _subjectUnderTest;
        private IRepositoryFactory _repositoryFactory;
        private ISavedSearchQueryRepository _savedSearchQueryRepository;

        [SetUp]
        public void Init()
        {
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _savedSearchQueryRepository = Substitute.For<ISavedSearchQueryRepository>();

            _repositoryFactory.GetSavedSearchQueryRepository(_WKSP_ID).Returns(_savedSearchQueryRepository);
            _subjectUnderTest = new SavedSearchFinderController(_repositoryFactory);

            _subjectUnderTest.Request = new HttpRequestMessage();
            _subjectUnderTest.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [Test]
        public void ItShouldGetSavedSearches()
        {
            // Arrange
            var savesSearchesDto = new SavedSearchDTO()
            {
                ArtifactId = 1234,
                Name = "Name"
            };

            _savedSearchQueryRepository.RetrievePublicSavedSearches().Returns(new[] {savesSearchesDto});

            // Act

            HttpResponseMessage response = _subjectUnderTest.Get(_WKSP_ID);

            var objectContent = response.Content as ObjectContent;
            var enumerableResponse = (IEnumerable<SavedSearchModel>) objectContent?.Value;

            var responseList = enumerableResponse.ToList();

            Assert.That(responseList.Count, Is.EqualTo(1));
            Assert.That(responseList[0].Value, Is.EqualTo(savesSearchesDto.ArtifactId));
            Assert.That(responseList[0].DisplayName, Is.EqualTo(savesSearchesDto.Name));
        }
    }
}
