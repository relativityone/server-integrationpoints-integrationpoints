using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;
using Relativity;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    public class ProductionControllerTests : TestBase
    {
        private ProductionController _sut;
        private IProductionManager _productionManager;
        private IManagerFactory _managerFactory;

        [SetUp]
        public override void SetUp()
        {
            _managerFactory = Substitute.For<IManagerFactory>();
            _productionManager = Substitute.For<IProductionManager>();
            _sut = new ProductionController(_managerFactory, _productionManager);
            _sut.Request = new HttpRequestMessage();
            _sut.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [Test]
        public void ItShouldReturnSortedProductions()
        {
            // Arrange
            var production1 = new ProductionDTO
            {
                ArtifactID = "1",
                DisplayName = "A"
            };
            var production2 = new ProductionDTO
            {
                ArtifactID = "2",
                DisplayName = "B"
            };
            var expectedResult = new List<ProductionDTO> { production1, production2 };

            var productions = new List<ProductionDTO> { production2, production1 };


            _productionManager.GetProductionsForExport(0).ReturnsForAnyArgs(productions);

            // Act
            HttpResponseMessage responseMessage = _sut.GetProductionsForExport(0);
            IEnumerable<ProductionDTO> actualResult = ExtractResponse(responseMessage);

            // Assert
            CollectionAssert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void ItShouldReturnImportProductions()
        {
            // Arrange
            var production1 = new ProductionDTO
            {
                ArtifactID = "1",
                DisplayName = "A"
            };
            var production2 = new ProductionDTO
            {
                ArtifactID = "2",
                DisplayName = "B"
            };

            var production3 = new ProductionDTO
            {
                ArtifactID = "0",
                DisplayName = string.Empty
            };

            // It will sort alphabetically and filter out the empty production
            var expectedResult = new List<ProductionDTO> { production1, production2 };

            var productions = new List<ProductionDTO> { production2, production1, production3 };

            _productionManager.GetProductionsForImport(0).ReturnsForAnyArgs(productions);

            // Act
            var responseMessage = _sut.GetProductionsForImport(0, null);
            var actualResult = ExtractResponse(responseMessage);

            // Assert
            CollectionAssert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void ItShouldSortProductionsIgnoringCase()
        {
            // Arrange
            var production1 = new ProductionDTO
            {
                ArtifactID = "1",
                DisplayName = "abc"
            };
            var production2 = new ProductionDTO
            {
                ArtifactID = "2",
                DisplayName = "Bcd"
            };
            var expectedResult = new List<ProductionDTO> { production1, production2 };

            var productions = new List<ProductionDTO> { production2, production1 };

            _productionManager.GetProductionsForExport(0).ReturnsForAnyArgs(productions);

            // Act
            var responseMessage = _sut.GetProductionsForExport(0);
            var actualResult = ExtractResponse(responseMessage);

            // Assert
            CollectionAssert.AreEqual(expectedResult, actualResult);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ItShouldCheckProductionAddPermission(bool hasPermission)
        {
            // Arrange
            IPermissionManager permissionManager = Substitute.For<IPermissionManager>();
            permissionManager.UserHasArtifactTypePermission(Arg.Any<int>(), (int)ArtifactType.Production, ArtifactPermission.Create).Returns(hasPermission);
            _managerFactory.CreatePermissionManager().Returns(permissionManager);

            // Act
            HttpResponseMessage responseMessage = _sut.CheckProductionAddPermission(null, 0);
            var objectContent = responseMessage.Content as ObjectContent<bool>;
            bool? actualResult = objectContent?.Value as bool?;

            // Assert
            Assert.AreEqual(hasPermission, actualResult);
        }

        private IEnumerable<ProductionDTO> ExtractResponse(HttpResponseMessage response)
        {
            var objectContent = response.Content as ObjectContent;
            var list = (IEnumerable<ProductionDTO>)objectContent?.Value;

            return list;
        }
    }
}
