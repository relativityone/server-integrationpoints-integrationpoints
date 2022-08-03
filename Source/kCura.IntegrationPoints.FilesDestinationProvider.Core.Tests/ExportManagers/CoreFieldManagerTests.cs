using kCura.EDDS.WebAPI.FieldManagerBase;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API.Foundation;
using FieldCategory = kCura.EDDS.WebAPI.FieldManagerBase.FieldCategory;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.ExportManagers
{
    [TestFixture, Category("Unit")]
    public class CoreFieldManagerTests
    {
        private IFieldRepository _fieldRepository;
        private CoreFieldManager _sut;
        private const int _APP_ID = 123;

        [SetUp]
        public void SetUp()
        {
            _fieldRepository = Substitute.For<IFieldRepository>();
            IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
            repositoryFactory.GetFieldRepository(_APP_ID).Returns(_fieldRepository);

            _sut = new CoreFieldManager(repositoryFactory);
        }

        [Test]
        public void ItShouldReadFieldFromFieldRepository()
        {
            // Arrange
            const int fieldArtifactId = 4643543;

            // Act
            _sut.Read(_APP_ID, fieldArtifactId);

            // Assert
            _fieldRepository.Received().Read(fieldArtifactId);
        }

        [Test]
        public void ItShouldReturnFieldWithCorrectProperties()
        {
            // Arrange
            const int fieldArtifactId = 4643543;
            const int expectedArtifactId = 9734;
            global::Relativity.API.Foundation.FieldCategory returnedCategory = global::Relativity.API.Foundation.FieldCategory.FolderName;
            FieldCategory expectedCategory = FieldCategory.FolderName;

            IField fieldReturnedFromRepository = Substitute.For<IField>();
            fieldReturnedFromRepository.ArtifactID.Returns(expectedArtifactId);
            fieldReturnedFromRepository.FieldCategory.Returns(returnedCategory);

            _fieldRepository.Read(fieldArtifactId).Returns(fieldReturnedFromRepository);

            // Act
            Field actualResult =_sut.Read(_APP_ID, fieldArtifactId);

            // Assert
            Assert.AreEqual(expectedArtifactId, actualResult.ArtifactID);
            Assert.AreEqual(expectedCategory, actualResult.FieldCategory);
        }
    }
}