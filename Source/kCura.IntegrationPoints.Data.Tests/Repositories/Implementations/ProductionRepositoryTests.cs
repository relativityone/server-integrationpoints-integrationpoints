using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Productions.Services;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class ProductionRepositoryTests
    {
        private const int _WORKSPACE_ARTIFACT_ID = 1016868;
        private const int _PRODUCTION_ARTIFACT_ID = 1234;
        private IServicesMgr _servicesMgr;
        private IProductionManager _productionManager;
        private IProductionService _productionService;
        private IProductionRepository _instance;

        [SetUp]
        public void SetUp()
        {
            _productionManager = Substitute.For<IProductionManager>();
            _productionService = Substitute.For<IProductionService>();

            _servicesMgr = Substitute.For<IServicesMgr>();
            _servicesMgr.CreateProxy<IProductionManager>(ExecutionIdentity.CurrentUser).Returns(_productionManager);
            _servicesMgr.CreateProxy<IProductionService>(ExecutionIdentity.CurrentUser).Returns(_productionService);

            _instance = new ProductionRepository(_servicesMgr);
        }

        [Test]
        public void ItShouldRetrieveExistingProduction()
        {
            // Arrange 
            const int expectedArtifactId = 4321;
            const string expectedName = "productionName";
            var production = new Production()
            {
                ArtifactID = expectedArtifactId,
                Name = expectedName
            };
            _productionManager.ReadSingleAsync(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Returns(production);

            // Act
            ProductionDTO actualResult = _instance.GetProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

            // Assert
            Assert.That(actualResult.ArtifactID, Is.EqualTo(expectedArtifactId.ToString()));
            Assert.That(actualResult.DisplayName, Is.EqualTo(expectedName));
        }

        [Test]
        public void ItShouldNoRetrieveAndThrowException()
        {
            // Arrange
            _productionManager.ReadSingleAsync(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Result.Throws(new Exception());

            // Act & Assert
            Assert.Throws<Exception>(() => _instance.GetProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID), "Unable to retrieve production");
        }

        [Test]
        public async Task GetProductionsForExportAsync_ItShouldRetrieveAllAvailableProductions()
        {
            // Arrange
            List<ProductionDTO> productionsDtos = GetProductionDtos();
            DataSet productionsDataSet = CreateDataSet(productionsDtos);
            _productionService.RetrieveProducedByContextArtifactIDAsync(_WORKSPACE_ARTIFACT_ID, String.Empty).Returns(new DataSetWrapper(productionsDataSet));

            // Act
            IEnumerable<ProductionDTO> actualResults = await _instance.GetProductionsForExport(_WORKSPACE_ARTIFACT_ID)
                .ConfigureAwait(false);

            // Assert
            actualResults.Select(x => x.DisplayName).ShouldAllBeEquivalentTo(productionsDtos.Select(x => x.DisplayName));
            actualResults.Select(x => x.ArtifactID).ShouldAllBeEquivalentTo(productionsDtos.Select(x => x.ArtifactID));
        }
        
        [Test]
        public void GetProductionsForExportAsync_ItShouldThrowExceptionWhenExceptionIsThrown()
        {
            // Arrange 
            _productionService.RetrieveProducedByContextArtifactIDAsync(_WORKSPACE_ARTIFACT_ID, String.Empty).Throws<Exception>();

            // Act
            Func<Task> function = async () => await _instance.GetProductionsForExport(_WORKSPACE_ARTIFACT_ID);

            // Assert
            function.ShouldThrow<Exception>($"Unable to retrieve productions for workspaceId: {_WORKSPACE_ARTIFACT_ID}");
        }

        [Test]
        public async Task GetProductionsForImportAsync_ItShouldRetrieveAllAvailableProductions()
        {
            // Arrange
            List<ProductionDTO> productionsDtos = GetProductionDtos();
            DataSet productionsDataSet = CreateDataSet(productionsDtos);
            _productionService.RetrieveImportEligibleByContextArtifactIDAsync(_WORKSPACE_ARTIFACT_ID, String.Empty).Returns(new DataSetWrapper(productionsDataSet));

            // Act
            IEnumerable<ProductionDTO> actualResults = await _instance.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID)
                .ConfigureAwait(false);

            // Assert
            actualResults.Select(x => x.DisplayName).ShouldAllBeEquivalentTo(productionsDtos.Select(x => x.DisplayName));
            actualResults.Select(x => x.ArtifactID).ShouldAllBeEquivalentTo(productionsDtos.Select(x => x.ArtifactID));
        }

        [Test]
        public void GetProductionsForImportAsync_ItShouldThrowExceptionWhenExceptionIsThrown()
        {
            // Arrange 
            _productionService.RetrieveImportEligibleByContextArtifactIDAsync(_WORKSPACE_ARTIFACT_ID, String.Empty).Throws<Exception>();

            // Act
            Func<Task> function = async () => await _instance.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID);

            // Assert
            function.ShouldThrow<Exception>($"Unable to retrieve productions for workspaceId: {_WORKSPACE_ARTIFACT_ID}");
        }

        [Test]
        public void ItShouldCreateSingleProduction()
        {
            // Arrange
            const int expectedResult = 1000;
            var production = new Production(); 
            _productionManager.CreateSingleAsync(_WORKSPACE_ARTIFACT_ID, production).Returns(expectedResult);

            // Act
            int actualResult = _instance.CreateSingle(_WORKSPACE_ARTIFACT_ID, production);

            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldNotCreateSingleProduction()
        {
            // Arrange
            var production = new Production();
            _productionManager.CreateSingleAsync(_WORKSPACE_ARTIFACT_ID, production).Result.Throws(new Exception());

            // Act & Assert
            Assert.Throws<Exception>(() => _instance.CreateSingle(_WORKSPACE_ARTIFACT_ID, production),
                "Unable to create production");
        }

        private static DataSet CreateDataSet(IEnumerable<ProductionDTO> productionDtos)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ArtifactId", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            DataSet filesDataSet = new DataSet();
            foreach (var fileDto in productionDtos)
            {
                AddFileDtoToDataTable(dataTable, fileDto);
            }
            filesDataSet.Tables.Add(dataTable);
            return filesDataSet;
        }

        private static void AddFileDtoToDataTable(DataTable dataTable, ProductionDTO productionDto)
        {
            object[] values = { productionDto.ArtifactID, productionDto.DisplayName };
            dataTable.Rows.Add(values);
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
