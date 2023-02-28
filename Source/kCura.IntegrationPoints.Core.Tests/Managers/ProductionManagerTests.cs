using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Productions.Services;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class ProductionManagerTests
    {
        private kCura.IntegrationPoints.Core.Managers.IProductionManager _sut;
        private IRepositoryFactory _repositoryFactory;
        private IProductionRepository _productionRepository;
        private IAPILog _logger;
        private const int _WORKSPACE_ARTIFACT_ID = 101810;
        private const int _PRODUCTION_ARTIFACT_ID = 987654;

        [SetUp]
        public void SetUp()
        {
            _productionRepository = Substitute.For<IProductionRepository>();

            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _repositoryFactory.GetProductionRepository(_WORKSPACE_ARTIFACT_ID).Returns(_productionRepository);

            _logger = Substitute.For<IAPILog>();

            _sut = new kCura.IntegrationPoints.Core.Managers.Implementations.ProductionManager(_repositoryFactory, _logger);
        }

        [Test]
        public void ItShouldRetrieveProduction()
        {
            // Arange
            const string expectedArtifactId = "123456";
            const string expectedDisplayName = "expectedDisplayName";
            var production = new ProductionDTO()
            {
                ArtifactID = expectedArtifactId,
                DisplayName = expectedDisplayName
            };
            _productionRepository.GetProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Returns(production);

            // Act
            ProductionDTO actual = _sut.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

            // Assert
            Assert.That(actual.ArtifactID, Is.EqualTo(expectedArtifactId));
            Assert.That(actual.DisplayName, Is.EqualTo(expectedDisplayName));
        }

        [Test]
        public void ItShouldNotRetrieveAndThrowException()
        {
            // Arrange
            _productionRepository.GetProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Throws(new Exception());

            // Act & Assert
            Assert.Throws<Exception>(() => _sut.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID));
        }

        [Test]
        public void ItShouldCreateSingleProduction()
        {
            // Arrange
            var production = new Production();
            const int expectedResult = 99;
            _productionRepository.CreateSingle(_WORKSPACE_ARTIFACT_ID, production).Returns(expectedResult);

            // Act
            int actualResult = _sut.CreateSingle(_WORKSPACE_ARTIFACT_ID, production);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldNotCreateSingleProductionAndThrowException()
        {
            // Arrange
            var production = new Production();
            _productionRepository.CreateSingle(_WORKSPACE_ARTIFACT_ID, production).Throws(new Exception());

            // Act & Assert
            Assert.Throws<Exception>(() => _sut.CreateSingle(_WORKSPACE_ARTIFACT_ID, production),
                "Unable to create production");
        }

        [Test]
        public void IsProductionInDestinationWorkspaceAvailable_ShouldReturnFalse_WhenProductionManagerThrowsException()
        {
            _productionRepository.GetProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Throws<Exception>();

            // act
            bool result = _sut.IsProductionInDestinationWorkspaceAvailable(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsProductionEligibleForImport_ShouldReturnTrue_WhenProductionManagerReturnsThisProduction()
        {
            // Arrange
            ProductionDTO productionDto = new ProductionDTO()
            {
                ArtifactID = _PRODUCTION_ARTIFACT_ID.ToString()
            };
            _productionRepository.GetProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Returns(productionDto);

            // Act
            bool result = _sut.IsProductionInDestinationWorkspaceAvailable(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsProductionInDestinationWorkspaceAvailable_ShouldReturnFalse_WhenProductionManagerReturnsNullObject()
        {
            // Arrange
            ProductionDTO productionDto = null;
            _productionRepository.GetProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Returns(productionDto);

            // Act
            bool result = _sut.IsProductionInDestinationWorkspaceAvailable(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

            // assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsProductionEligibleForImport_ShouldReturnFalse_WhenProductionManagerReturnsEmptyList()
        {
            // Arrange
            List<ProductionDTO> expected = GetProductionDtos();

            _productionRepository.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID).Returns(expected);

            // Act
            bool result = _sut.IsProductionEligibleForImport(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsProductionEligibleForImport_ShouldReturnFalse_WhenProductionManagerReturnsListWithoutThisProduction()
        {
            // Arrange
            _productionRepository.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID).Returns(new List<ProductionDTO>()
            {
                new ProductionDTO()
                {
                    ArtifactID = "99999"
                }
            });

            // Act
            bool result = _sut.IsProductionEligibleForImport(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsProductionEligibleForImport_ShouldReturnFalse_WhenProductionManagerThrowsException()
        {
            _productionRepository.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID).Throws<Exception>();

            // Act
            bool result = _sut.IsProductionEligibleForImport(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ItShouldGetProductionsForExport()
        {
            // Arrange
            List<ProductionDTO> productionDtos = GetProductionDtos();
            _productionRepository.GetProductionsForExport(_WORKSPACE_ARTIFACT_ID).Returns(productionDtos);

            // Act
            List<ProductionDTO> actualProductionDto = _sut.GetProductionsForExport(_WORKSPACE_ARTIFACT_ID).ToList();

            // Assert
            Assert.That(actualProductionDto.Count, Is.EqualTo(productionDtos.Count));
            Assert.That(actualProductionDto.Select(x => x.ArtifactID), Is.EqualTo(productionDtos.Select(x => x.ArtifactID)));
            Assert.That(actualProductionDto.Select(x => x.ArtifactID), Is.EqualTo(productionDtos.Select(x => x.ArtifactID)));
        }

        [Test]
        public void ItShouldGetProductionsForImport()
        {
            // Arrange
            List<ProductionDTO> productionDtos = GetProductionDtos();
            _productionRepository.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID).Returns(productionDtos);

            // Act
            List<ProductionDTO> actualProductionDto = _sut.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID).ToList();

            // Assert
            Assert.That(actualProductionDto.Count, Is.EqualTo(productionDtos.Count));
            Assert.That(actualProductionDto.Select(x => x.ArtifactID), Is.EqualTo(productionDtos.Select(x => x.ArtifactID)));
            Assert.That(actualProductionDto.Select(x => x.ArtifactID), Is.EqualTo(productionDtos.Select(x => x.ArtifactID)));
        }

        [Test]
        public void IsProductionInDestinationWorkspaceAvailable_ShouldReturnTrue_WhenProductionManagerReturnsNonNullObject()
        {
            // Arrange
            ProductionDTO productionDto = new ProductionDTO()
            {
                ArtifactID = _PRODUCTION_ARTIFACT_ID.ToString()
            };

            _productionRepository.GetProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Returns(productionDto);

            // Act
            bool result = _sut.IsProductionInDestinationWorkspaceAvailable(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

            // Assert
            Assert.IsTrue(result);
        }

        private List<ProductionDTO> GetProductionDtos()
        {
            List<ProductionDTO> productionsDtos = new List<ProductionDTO>
            {
                new ProductionDTO
                {
                    ArtifactID = "4321",
                    DisplayName = "Production 4321"
                },
                new ProductionDTO
                {
                    ArtifactID = "4322",
                    DisplayName = "Production 4322"
                },
                new ProductionDTO
                {
                    ArtifactID = "4323",
                    DisplayName = "Production 4323"
                },
                new ProductionDTO
                {
                    ArtifactID = "4324",
                    DisplayName = "Production 4324"
                }
            };

            return productionsDtos;
        }
    }
}
